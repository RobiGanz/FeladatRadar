window.themeHelper = {
    getTheme: function () {
        return localStorage.getItem('fr-theme') || 'light';
    },
    setTheme: function (theme) {
        localStorage.setItem('fr-theme', theme);
    }
};

window.timerHelper = {
    getFocusMinutes: function () {
        return parseInt(localStorage.getItem('fr-focus-minutes') || '25');
    },
    getBreakMinutes: function () {
        return parseInt(localStorage.getItem('fr-break-minutes') || '5');
    },
    getLongBreakMinutes: function () {
        return parseInt(localStorage.getItem('fr-longbreak-minutes') || '15');
    },
    setFocusMinutes: function (val) {
        localStorage.setItem('fr-focus-minutes', val);
    },
    setBreakMinutes: function (val) {
        localStorage.setItem('fr-break-minutes', val);
    },
    setLongBreakMinutes: function (val) {
        localStorage.setItem('fr-longbreak-minutes', val);
    }
};

window.groupHelper = {
    getLastUsed: function (groupId) {
        return parseInt(localStorage.getItem('fr-group-lastused-' + groupId) || '0');
    },
    setLastUsed: function (groupId, ts) {
        localStorage.setItem('fr-group-lastused-' + groupId, ts.toString());
    }
};
