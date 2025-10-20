window.loginHelpers = {
    saveItemToLocal: function (name, value) {
        console.log("Saving " + value);
        localStorage.setItem(name, value.toString());
    },

    getItemFromLocal: function (name) {
        console.log("Loading " + name);
        return localStorage.getItem(name);
    },

    deleteItemFromLocal: function (name) {
        localStorage.removeItem(name);
    },
};

window.maxHeight = {
    add40Height: function (classId) {
        let uploadSquare = document.querySelector("." + classId);
        let currentHeight = parseInt(window.getComputedStyle(uploadSquare).height, 10) || 0;
        uploadSquare.style.height = (currentHeight + 40) + "px";
    },

    resetHeight: function (classId) {
        document.querySelector("." + classId).style.height = "251px";
    },

    equalHeight: function (classId) {
        let equalHeightElms = document.querySelectorAll(".js-equal-height-element");

        let highestElm = 0;
        equalHeightElms.forEach(element => {
            if (element.offsetHeight > highestElm) {
                console.log("New record height: " + element.offsetHeight);
                highestElm = element.offsetHeight;
            }
            console.log("Height: "+element.offsetHeight);
        })
        
        equalHeightElms.forEach(element => {
            element.style.minHeight = highestElm + "px";
        })
    }
}