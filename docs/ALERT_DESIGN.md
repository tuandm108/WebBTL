# Alert dialog mechanism (research and proposal)

Status: **partly built**. Built: server-side alert episodes with deadband (demo profile), SignalR push, a modal summary dialog on app open, a live dialog for critical alerts and sensor faults, nav badge and the history page. Not built: acknowledge (needs F08), on-delay, re-notify and shelving, toast for warnings, sound, device-offline alerts. Sections below are the original research; "Proposed mechanism" items 1-3 are implemented except acknowledgement.

## What the research says

| Topic | Finding | Source |
|---|---|---|
| Component choice | Interrupt (modal) only when the user must decide or work is blocked; persist (banner) while a state matters; auto-dismiss (toast) for confirmations needing no response. Overuse trains users to ignore alerts. | [Siemens iX messaging](https://ix.siemens.io/blog/messaging), [UX Movement](https://uxmovement.com/forms/the-3-types-of-alerts-and-how-to-use-them-correctly/) |
| Accessibility | `role="alert"` for feedback needing no input; `role="alertdialog"` + `aria-modal="true"` when a response is expected. Focus moves into the dialog, stays trapped, returns to the opener on close. Needs an accessible name and `aria-describedby` for the message. | [Deque](https://www.deque.com/blog/aria-modal-alert-dialogs-a11y-support-series-part-2/), [MDN dialog role](https://developer.mozilla.org/en-US/docs/Web/Accessibility/ARIA/Roles/dialog_role) |
| Industrial alarm practice | An alarm must target the operator, signal an abnormal condition and need a timely response. Priority comes from consequence and time to respond. Deadband and on/off delay stop chattering; shelving and flood suppression handle bursts. Vendor summaries only; the ISA-18.2 text itself was not found. | [exida: rationalization](https://exida.com/Blog/rationalize-your-alarm-management-problems-away1), [exida: deadband](https://www.exida.com/Blog/why-should-i-use-an-alarm-deadband) |
| Threshold values | Barn gas limits vary by species and housing: NH3 5-20 ppm typical in swine, 33-53 ppm in laying-hen houses; H2S 10 ppm 8 h limit (Ontario), 3 ppm pig-zone maximum (Iowa State fact sheet). No source for temperature/humidity setpoints was found. Hence D-003. | [Ontario H2S](https://www.ontario.ca/page/hydrogen-sulphide-livestock-facilities), [Poultry Producer](https://poultryproducer.com/air-quality-inside-livestock-barns/), [Pork.org fact sheet](https://employeesafety.pork.org/content/Additional%20Resources/Safety%20in%20swine%20production%20systems%20%20(toxic%20gases)%20Fact%20Sheet.pdf) |
| Sensor reliability | Low-cost H2S monitors can fail their high-level alarm in swine-barn conditions; bump tests are recommended. A silent sensor must raise its own alarm. | [CDC / PubMed 29140644](https://pubmed.ncbi.nlm.nih.gov/29140644/) |

Gaps: no empirical study on alert-fatigue thresholds, and the flood figure ("more than 10 alarms in 10 minutes") comes from a vendor blog.

## Proposed mechanism (not built)

1. **Backend raises alerts, the browser only shows them.** One open alert per `barn + metric + severity` (partial unique index from DATABASE_DESIGN.md); `alertChanged` over SignalR; REST reload on reconnect. State: `Active (unacked) -> Acknowledged -> Cleared -> Closed`, every change in the append-only `AlertEvent`.
2. **Severity decides the component.**
   - Warning: no interruption. Row tint, chip, nav badge with the open count (built; the toast is not).
   - Critical: (a persistent banner was built, then removed on request) plus **one** modal `alertdialog` per new critical alert listing barn, metric, value, level, since when. Several at once collapse into one dialog (list, worst first), never a stack.
   - Sensor fault or lost signal: its own alert type, never a threshold alert (invalid and stale data already map to normal severity).
3. **Dialog behaviour.** Focus lands on the first action ("Xac nhan"); focus trapped; Esc or close hides the dialog but leaves the alert active in the banner (dismiss is not acknowledge); focus returns to the opener; text plus icon plus colour, never colour alone; mobile uses the full-screen sheet already used by the barn picker.
4. **Anti-fatigue rules.** Deadband (clear only after the value falls a margin below the level), on-delay (condition must hold N seconds), no repeat popups for the same open alert (re-notify only as an escalation after M unacknowledged minutes), shelve with reason and expiry, flood guard that groups by barn.
5. **Audit.** Acknowledge records the user; needs F08 (authentication) first.
6. **Not in scope here.** Sound (browsers block autoplay without a user gesture) and background-tab notifications are optional later. A confirmation dialog for control commands (F09) is a separate flow with its own interlock rules.

## Open questions for the owner
- Real threshold profiles per species and who approves them.
- On-delay, deadband and re-notify intervals.
- Who may acknowledge, and whether a critical alert needs a note.

## Interruption control (built)
The digest dialog is scheduled instead of firing on every event, because interrupting for each alert was found disruptive. The user picks, per browser: once a day at a fixed time (default 23:59), every N minutes, immediately for critical alerts and sensor faults, or never; plus an optional digest on open. The rules are pure functions in `AlertDisplaySchedule` (Contracts) with unit tests. A persistent banner for open critical alerts was built and then removed at the owner's request. Consequence: with a scheduled digest an urgent condition reaches the user only through the unread badge and the history page until the digest is due, so "immediately" is the mode to choose when that delay is unacceptable. Daily times use the server's local time.

## Read state (built)
`Alert.ReadAt` is set by `POST /api/alerts/mark-read` (explicit ids, or `all` with the history filters) and cleared when the alert escalates, because that is new information. It is shared by every browser until accounts exist (F08 should add who and when, and an acknowledge step). Read affects only attention aids: the nav badge counts unread alerts and the digest dialog lists unread ones. Changes are pushed to all browsers as the SignalR event `alertsRead`.

The history page also shows the current number and species of animals assigned to each alert's barn, plus `ReadAt` when present. Animal assignment was added after the original alert schema, so old episodes have no animal roster snapshot. The UI labels these figures as current barn data; it does not claim an individual animal caused or experienced the alert.
