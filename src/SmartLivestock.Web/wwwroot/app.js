// Thin wrappers so Blazor can drive the native <dialog> element.
window.slDialog = {
    show: el => { if (el && !el.open) el.showModal(); },
    close: el => { if (el && el.open) el.close(); }
};
