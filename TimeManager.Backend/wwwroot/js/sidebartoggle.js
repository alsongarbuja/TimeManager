function sidebarToggle() {
    const sidebar = document.querySelector('.sidebar');
    sidebar.classList.toggle('sidebar-open');
}

document.addEventListener('click', function (e) {
    const sidebar = document.querySelector('.sidebar');
    const isOpen = sidebar.classList.contains('sidebar-open');
    const clickedInsideSidebar = sidebar.contains(e.target);
    const clickedMenuBtn = e.target.closest('.menu-btn');

    if (isOpen && !clickedInsideSidebar && !clickedMenuBtn) {
        sidebar.classList.remove('sidebar-open');
    }
});