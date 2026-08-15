const button = document.getElementById("themeToggle");

if (localStorage.getItem("theme") === "dark") {

    document.body.classList.add("dark-mode");

    button.innerHTML = '<i class="bi bi-sun-fill"></i>';

}

button.addEventListener("click", () => {

    document.body.classList.toggle("dark-mode");

    if (document.body.classList.contains("dark-mode")) {

        localStorage.setItem("theme", "dark");

        button.innerHTML = '<i class="bi bi-sun-fill"></i>';

    }
    else {

        localStorage.setItem("theme", "light");

        button.innerHTML = '<i class="bi bi-moon-fill"></i>';

    }

});