(function () {
    "use strict";

    var dateInput = document.getElementById("eventDateInput");
    var festivalInput = document.getElementById("festivalNameInput");
    if (!dateInput || !festivalInput) return;

    // Tracks the last value *we* auto-filled, so we don't clobber something the user typed.
    var lastAutoSuggested = festivalInput.value;

    dateInput.addEventListener("change", function () {
        if (!dateInput.value) return;

        fetch("/SlideMaker/Presentation/SpecialDay?date=" + encodeURIComponent(dateInput.value))
            .then(function (r) { return r.json(); })
            .then(function (data) {
                var suggestion = data.name || "";
                if (festivalInput.value === lastAutoSuggested) {
                    festivalInput.value = suggestion;
                    lastAutoSuggested = suggestion;
                }
            })
            .catch(function () { /* keep whatever the user already has */ });
    });
})();
