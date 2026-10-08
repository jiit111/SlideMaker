(function () {
    "use strict";

    var frames = document.querySelectorAll(".slide-frame");
    if (frames.length === 0) return;

    var current = 0;
    var slideIds = window.SLIDE_IDS || [];
    var positionLabel = document.getElementById("slidePositionLabel");
    var currentSlideIdInputs = document.querySelectorAll(".current-slide-id");

    function render() {
        frames.forEach(function (frame, i) {
            frame.style.display = i === current ? "" : "none";
        });
        positionLabel.textContent = current + 1;
        currentSlideIdInputs.forEach(function (input) {
            input.value = slideIds[current] || "";
        });
    }

    document.getElementById("prevSlideBtn").addEventListener("click", function () {
        current = current > 0 ? current - 1 : frames.length - 1;
        render();
    });

    document.getElementById("nextSlideBtn").addEventListener("click", function () {
        current = current < frames.length - 1 ? current + 1 : 0;
        render();
    });

    render();
})();
