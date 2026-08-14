document.addEventListener("DOMContentLoaded", () => {
    initTomSelectors();
    observeForTomSelectors();
});

function initTomSelectors(root = document) {
    const selects = root.matches?.("select.tom-select")
        ? [root, ...root.querySelectorAll("select.tom-select")]
        : root.querySelectorAll("select.tom-select");

    selects.forEach(enhanceSelect);
}

function enhanceSelect(select) { 
    if (select.tomselect)
        return;

    const ts = new TomSelect(select, {
        create: false,
        allowEmptyOption: true,
        placeholder: select.dataset.placeholder,
        maxOptions: 500,
        plugins: ['clear_button']
    });

    ts.wrapper.classList.add("tm-select-wrapper");
}

function observeForTomSelectors() {
    const observer = new MutationObserver(mutations => {
        for (const mutation of mutations) {
            mutation.addedNodes.forEach(node => {
                if (node.nodeType === Node.ELEMENT_NODE) {
                    initTomSelectors(node);
                }
            });
        }
    });

    observer.observe(document.body, { childList: true, subtree: true });
}