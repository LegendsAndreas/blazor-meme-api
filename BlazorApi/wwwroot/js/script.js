window.loginHelpers = {
    saveItemToLocal: function (name, value) {
        console.log("Saving " + value + " as " + name);
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