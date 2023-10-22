window.sessionStorage = {
    getItem: (key) => {
        return sessionStorage.getItem(key);
    },
    setItem: (key, value) => {
        sessionStorage.setItem(key, JSON.stringify(value));
    },
    removeItem: (key) => {
        sessionStorage.removeItem(key);
    }
};
