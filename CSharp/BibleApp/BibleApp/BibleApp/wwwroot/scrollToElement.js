window.scrollToElement = (element) => {
    element.scrollIntoView({ behavior: 'smooth', block: 'start' });
};