
//— dark mode téma mentés

window.themeHelper = {
    getTheme: function () {
        return localStorage.getItem('fr-theme') || 'light';
    },
    setTheme: function (theme) {
        localStorage.setItem('fr-theme', theme);
    }
};
