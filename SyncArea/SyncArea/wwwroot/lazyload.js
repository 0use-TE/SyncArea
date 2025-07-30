window.initLazyLoad = function () {
    const images = document.querySelectorAll('.lazy-image');
    const observer = new IntersectionObserver((entries, observer) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                const img = entry.target;
                const dataSrc = img.getAttribute('data-src');
                if (dataSrc) {
                    img.src = dataSrc;
                    img.removeAttribute('data-src');
                    observer.unobserve(img);
                }
            }
        });
    }, { root: null, rootMargin: '0px', threshold: 0.1 });
    images.forEach(img => observer.observe(img));
};