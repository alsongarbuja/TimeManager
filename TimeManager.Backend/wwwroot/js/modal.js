function toggleModal() {
    const modal = document.querySelector(".modal-overlay");
    modal.classList.toggle("modal-overlay-visible");
}

document.querySelectorAll(".btn-modal").forEach((ele, key, p) => {
    ele.addEventListener("click", toggleModal)
})