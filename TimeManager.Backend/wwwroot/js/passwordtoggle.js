function togglePasswordVisibility(button) {
    const input = button.parentElement.querySelector('input');

    if (input.type === "password") {
        input.type = "text";
        button.innerHTML = "<i class='ri-eye-line'></i>";
    } else {
        input.type = "password";
        button.innerHTML = "<i class='ri-eye-off-line'></i>";
    }
}