function closeToast() {
    const toast = document.getElementById('toast-notification');
    if (toast) toast.remove();
}

document.addEventListener('DOMContentLoaded', function () {
    const toast = document.getElementById('toast-notification');
    if (toast) {
        setTimeout(() => {
            toast.style.transition = 'opacity 0.3s ease';
            toast.style.opacity = '0';
            setTimeout(() => toast.remove(), 300);
        }, 4000);
    }
});