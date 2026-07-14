// Function to toggle the accordian style report table for each employee
function toggleItem(trigger) {
    const item = trigger.closest('.rbu-item');
    const isOpen = item.classList.contains('rbu-open');
    item.classList.toggle('rbu-open', !isOpen);
    trigger.setAttribute('aria-expanded', String(!isOpen));
}

// Function to toggle the accordian style of a certian employee while scrolling down to their table
function jumpToUser(id) {
    if (!id) return;
    const item = document.getElementById(id);
    if (!item) return;
    item.classList.add('rbu-open');
    item.querySelector('.rbu-trigger').setAttribute('aria-expanded', 'true');
    setTimeout(() => item.scrollIntoView({ behavior: 'smooth', block: 'start' }), 50);
}

// Function to toggle report tables of all the employees at once
function toggleAll(btn) {
    const items = document.querySelectorAll('.rbu-item');
    const anyOpen = [...items].some(i => i.classList.contains('rbu-open'));
    items.forEach(item => {
        item.classList.toggle('rbu-open', !anyOpen);
        item.querySelector('.rbu-trigger').setAttribute('aria-expanded', String(!anyOpen));
    });
    btn.textContent = anyOpen ? 'Expand all' : 'Collapse all';
}

// Creating a fetch style query for report excel download form to add loading state
(function () {
    const form = document.getElementById('exportForm');
    const btn = document.getElementById('exportBtn');
    const icon = document.getElementById('exportBtnIcon');
    const label = document.getElementById('exportBtnLabel');

    let isDownloading = false;

    form.addEventListener('submit', async function (e) {
        e.preventDefault();

        if (isDownloading) return;
        isDownloading = true;

        btn.disabled = true;
        btn.classList.add('rg-back--loading');
        icon.classList.remove('ri-download-line');
        icon.classList.add('ri-loader-4-line', 'rg-spin');
        label.textContent = 'Generating…';

        try {
            const formData = new FormData(form);
            const response = await fetch(form.action, {
                method: 'POST',
                body: formData
            });

            if (!response.ok) {
                throw new Error(`Export failed with status ${response.status}`);
            }

            const blob = await response.blob();

            const disposition = response.headers.get('Content-Disposition') || '';
            const match = disposition.match(/filename\*?=(?:UTF-8'')?"?([^";]+)"?/i);
            const fileName = match ? decodeURIComponent(match[1]) : 'Report.xlsx';

            const url = window.URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = url;
            a.download = fileName;
            document.body.appendChild(a);
            a.click();
            a.remove();
            window.URL.revokeObjectURL(url);
        } catch (err) {
            console.error(err);
            alert('Something went wrong generating the report. Please try again.');
        } finally {
            isDownloading = false;
            btn.disabled = false;
            btn.classList.remove('rg-back--loading');
            icon.classList.remove('ri-loader-4-line', 'rg-spin');
            icon.classList.add('ri-download-line');
            label.textContent = 'Download Excel';
        }
    });
})();