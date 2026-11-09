// Photo carousel on the property cards: previous and next arrows change the photo and the dots
document.addEventListener('click', function (e) {
    var btn = e.target.closest('.sb-car-btn');
    if (!btn) return;

    e.preventDefault();

    var media = btn.closest('.sb-carousel');
    var slides = media.querySelectorAll('.sb-slide');
    var dots = media.querySelectorAll('.sb-dot');
    if (slides.length < 2) return;

    var index = 0;
    slides.forEach(function (slide, i) {
        if (slide.classList.contains('active')) index = i;
    });

    var next = btn.classList.contains('next')
        ? (index + 1) % slides.length
        : (index - 1 + slides.length) % slides.length;

    slides.forEach(function (slide, i) { slide.classList.toggle('active', i === next); });
    dots.forEach(function (dot, i) { dot.classList.toggle('active', i === next); });
});
