window.themeHelper = {
    getTheme: function () {
        var t = localStorage.getItem('fr-theme') || 'light';
        document.body.setAttribute('data-theme', t);
        return t;
    },
    setTheme: function (theme) {
        localStorage.setItem('fr-theme', theme);
        document.body.setAttribute('data-theme', theme);
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

window.confirmDialog = {
    show: function (message) {
        return window.confirm(message);
    }
};

(function () {
    var t = localStorage.getItem('fr-theme') || 'light';
    document.documentElement.setAttribute('data-theme', t);
    if (document.body) document.body.setAttribute('data-theme', t);
    else document.addEventListener('DOMContentLoaded', function () {
        document.body.setAttribute('data-theme', t);
    });
})();
