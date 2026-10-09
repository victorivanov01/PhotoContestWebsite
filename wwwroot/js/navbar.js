// Get elements for the burger menu
const burgerMenu = document.querySelector(".burger-menu");
const navList = document.querySelector("nav ul");

// Toggle the "show" class to display the menu when the burger icon is clicked
burgerMenu.addEventListener("click", () => {
    navList.classList.toggle("show");
});
