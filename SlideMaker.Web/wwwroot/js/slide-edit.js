(function () {
    "use strict";

    var editBtn = document.getElementById("editSlideBtn");
    var saveBtn = document.getElementById("editSlideSaveBtn");
    var body = document.getElementById("editSlideBody");
    if (!editBtn || !saveBtn || !body) return;

    var LABELS = {
        QuestionText: "Question",
        OptionsList: "Options (one per line)",
        AnswerText: "Answer",
        ExplanationText: "Explanation"
    };

    function getVisibleSlideFrame() {
        var frames = document.querySelectorAll(".slide-frame");
        for (var i = 0; i < frames.length; i++) {
            if (frames[i].style.display !== "none") return frames[i];
        }
        return null;
    }

    editBtn.addEventListener("click", function () {
        var frame = getVisibleSlideFrame();
        body.innerHTML = "";

        if (!frame) {
            body.innerHTML = '<p class="text-muted">No slide to edit.</p>';
            return;
        }

        var editableEls = frame.querySelectorAll('.el[data-editable="true"]');
        if (editableEls.length === 0) {
            body.innerHTML = '<p class="text-muted">Nothing editable on this slide (only the branding header/footer are here).</p>';
            return;
        }

        editableEls.forEach(function (el) {
            var type = el.getAttribute("data-element-type");
            var id = el.getAttribute("data-element-id");
            var label = LABELS[type] || type;

            var group = document.createElement("div");
            group.className = "mb-3";
            group.innerHTML =
                '<label class="form-label small text-muted mb-1">' + label + '</label>' +
                '<textarea class="form-control" rows="' + (type === "OptionsList" ? 4 : 2) + '" data-element-id="' + id + '"></textarea>';

            group.querySelector("textarea").value = el.innerText;
            body.appendChild(group);
        });
    });

    saveBtn.addEventListener("click", function () {
        var textareas = body.querySelectorAll("textarea[data-element-id]");
        if (textareas.length === 0) return;

        saveBtn.disabled = true;
        var requests = Array.from(textareas).map(function (textarea) {
            var elementId = textarea.getAttribute("data-element-id");
            var content = textarea.value;
            return window.SlideMaker.postForm("/SlideMaker/Presentation/UpdateSlideElement", {
                presentationId: window.PRESENTATION_ID,
                elementId: elementId,
                content: content
            }).then(function (resp) {
                if (resp.ok) {
                    document.querySelectorAll('.el[data-element-id="' + elementId + '"]').forEach(function (el) {
                        el.innerText = content;
                    });
                }
                return resp.ok;
            });
        });

        Promise.all(requests).then(function (results) {
            saveBtn.disabled = false;
            var modalEl = document.getElementById("editSlideModal");
            var modal = bootstrap.Modal.getOrCreateInstance(modalEl);
            modal.hide();

            if (results.every(Boolean)) {
                window.SlideMaker.showToast("Slide updated.");
            } else {
                window.SlideMaker.showToast("Some changes could not be saved.", "danger");
            }
        });
    });
})();
