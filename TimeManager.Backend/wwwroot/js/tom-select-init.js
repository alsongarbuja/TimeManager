document.addEventListener("DOMContentLoaded", () => {

    document.querySelectorAll("select.tom-select").forEach(select => {

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
    });

});