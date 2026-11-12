window.restaurantsAuth = {
    getSession: () => {
        const value = sessionStorage.getItem("restaurants_api_session");
        return value ? JSON.parse(value) : null;
    },
    setSession: session => sessionStorage.setItem("restaurants_api_session", JSON.stringify(session)),
    clearSession: () => sessionStorage.removeItem("restaurants_api_session")
};
