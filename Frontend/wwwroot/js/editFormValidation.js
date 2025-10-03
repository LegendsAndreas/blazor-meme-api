window.validationHelpers = {
    copyValidationSummaryAndActivateDum: function () {
        let validationSummary = document.querySelectorAll(".js-get-validation-content ul li");
        let dumValidator = document.querySelector(".js-dummy-validation");
        let liTags = "";

        validationSummary.forEach(element => {
            liTags += "<li>" + element.innerHTML + "</li>";
        });
        console.log(liTags);

        dumValidator.innerHTML = "<ul>" + liTags + "</ul>";
        dumValidator.classList.add("active");
    },

    emptyValidationSummary: function () {
        document.querySelector(".js-dummy-validation").classList.remove("active");
    }
}