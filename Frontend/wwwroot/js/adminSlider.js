document.addEventListener("DOMContentLoaded", function () {
    console.log("Fick");

    let slideout = document.querySelector(".js-slideout");
    let slideoutButton = document.querySelector(".js-slideout-button");

    if (!slideout || !slideoutButton) {
        console.log("Slideout or button not found");
        return;
    }

    slideoutButton.addEventListener("click", function () {
        slideout.classList.toggle("active");
    })

    let slideoutContent = document.querySelector(".js-click-anywhere-else");

    document.addEventListener("click", function(event) {
        if (!slideoutContent.contains(event.target) && !slideoutButton.contains(event.target) && slideout.classList.contains("active")) {
            slideout.classList.remove("active");
        }
    });
})

window.adminSlider = {
    toggleAdminSlider: function () {
        let slideout = document.querySelector(".js-slideout");
        slideout.classList.toggle("active");
    },
    closeAdminSlider: function () {
        let slideout = document.querySelector(".js-slideout");
        let slideoutButton = document.querySelector(".js-slideout-button");
        let slideoutContent = document.querySelector(".js-click-anywhere-else");

        document.addEventListener("click", function(event) {
            if (!slideoutContent.contains(event.target) && !slideoutButton.contains(event.target) && slideout.classList.contains("active")) {
                slideout.classList.remove("active");
            } else {
                console.log("Not active");
            }
        });
    }
}