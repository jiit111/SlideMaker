(function () {
    "use strict";

    var listEl = document.getElementById("questionList");
    var emptyStateEl = document.getElementById("emptyState");
    var presentationId = document.getElementById("presentationId").value;
    var baseUrl = "/SlideMaker/Question/";

    function renumber() {
        var cards = listEl.querySelectorAll(".question-card");
        cards.forEach(function (card, index) {
            card.querySelector(".q-number").textContent = index + 1;
            card.querySelector('[data-field="SortOrder"]').value = index + 1;
        });
        emptyStateEl.classList.toggle("d-none", cards.length > 0);
    }

    function collectCardData(card) {
        var data = {
            Id: card.getAttribute("data-id"),
            QuestionText: card.querySelector('[data-field="QuestionText"]').value,
            SortOrder: card.querySelector('[data-field="SortOrder"]').value,
            CorrectAnswer: card.querySelector('[data-field="CorrectAnswer"]').value,
            Difficulty: card.querySelector('[data-field="Difficulty"]').value,
            Language: card.querySelector('[data-field="Language"]').value,
            Explanation: card.querySelector('[data-field="Explanation"]').value
        };
        var optionInputs = card.querySelectorAll('[data-field="Option"]');
        optionInputs.forEach(function (input, i) {
            data["Options[" + i + "].Label"] = input.getAttribute("data-label");
            data["Options[" + i + "].Text"] = input.value;
        });
        return data;
    }

    function optionRowHtml(label, text) {
        return '<div class="col-md-6"><div class="input-group input-group-sm">' +
            '<span class="input-group-text opt-label">' + label + '</span>' +
            '<input class="form-control q-field" data-field="Option" data-label="' + label + '" value="' + escapeAttr(text || "") + '" />' +
            "</div></div>";
    }

    function escapeAttr(value) {
        return (value || "").replace(/&/g, "&amp;").replace(/"/g, "&quot;").replace(/</g, "&lt;");
    }

    function buildCardElement(card) {
        var wrapper = document.createElement("div");
        wrapper.className = "card question-card mb-3";
        wrapper.setAttribute("data-id", card.id);

        var options = (card.options && card.options.length > 0)
            ? card.options
            : [{ label: "A", text: "" }, { label: "B", text: "" }, { label: "C", text: "" }, { label: "D", text: "" }];

        wrapper.innerHTML =
            '<div class="card-header d-flex justify-content-between align-items-center">' +
            '<span>Question <span class="q-number">' + (card.sortOrder || 0) + '</span></span>' +
            '<div class="btn-group btn-group-sm">' +
            '<button type="button" class="btn btn-outline-secondary" data-action="move-up" title="Move Up"><i class="bi bi-arrow-up"></i></button>' +
            '<button type="button" class="btn btn-outline-secondary" data-action="move-down" title="Move Down"><i class="bi bi-arrow-down"></i></button>' +
            '<button type="button" class="btn btn-outline-secondary" data-action="duplicate" title="Duplicate"><i class="bi bi-copy"></i></button>' +
            '<button type="button" class="btn btn-outline-secondary" data-action="split" title="Split"><i class="bi bi-scissors"></i></button>' +
            '<button type="button" class="btn btn-outline-secondary" data-action="merge" title="Merge with next"><i class="bi bi-union"></i></button>' +
            '<button type="button" class="btn btn-outline-secondary" data-action="regenerate" title="Regenerate with AI"><i class="bi bi-arrow-repeat"></i></button>' +
            '<button type="button" class="btn btn-outline-danger" data-action="delete" title="Delete"><i class="bi bi-trash"></i></button>' +
            "</div></div>" +
            '<div class="card-body">' +
            '<input type="hidden" class="q-field" data-field="SortOrder" value="' + (card.sortOrder || 0) + '" />' +
            '<div class="mb-2"><label class="form-label small text-muted mb-1">Question Text</label>' +
            '<textarea class="form-control q-field" data-field="QuestionText" rows="2">' + escapeAttr(card.questionText) + "</textarea></div>" +
            '<div class="row g-2 mb-2">' + options.map(function (o) { return optionRowHtml(o.label, o.text); }).join("") + "</div>" +
            '<div class="row g-2 mb-2">' +
            '<div class="col-md-3"><label class="form-label small text-muted mb-1">Answer</label>' +
            '<input class="form-control form-control-sm q-field" data-field="CorrectAnswer" value="' + escapeAttr(card.correctAnswer) + '" /></div>' +
            '<div class="col-md-3"><label class="form-label small text-muted mb-1">Difficulty</label>' +
            '<select class="form-select form-select-sm q-field" data-field="Difficulty">' +
            ["Easy", "Medium", "Hard", "Mixed"].map(function (d) {
                return '<option value="' + d + '"' + (d === card.difficulty ? " selected" : "") + ">" + d + "</option>";
            }).join("") + "</select></div>" +
            '<div class="col-md-3"><label class="form-label small text-muted mb-1">Language</label>' +
            '<select class="form-select form-select-sm q-field" data-field="Language">' +
            ["English", "Hindi", "Hinglish", "Bilingual"].map(function (l) {
                return '<option value="' + l + '"' + (l === card.language ? " selected" : "") + ">" + l + "</option>";
            }).join("") + "</select></div>" +
            '<div class="col-md-3 d-flex align-items-end"><button type="button" class="btn btn-sm btn-success w-100" data-action="save"><i class="bi bi-check-lg me-1"></i>Save</button></div>' +
            "</div>" +
            '<div><label class="form-label small text-muted mb-1">Explanation</label>' +
            '<textarea class="form-control q-field" data-field="Explanation" rows="2">' + escapeAttr(card.explanation) + "</textarea></div>" +
            "</div>";

        return wrapper;
    }

    function updateCardFields(cardEl, card) {
        cardEl.querySelector('[data-field="QuestionText"]').value = card.questionText || "";
        cardEl.querySelector('[data-field="CorrectAnswer"]').value = card.correctAnswer || "";
        cardEl.querySelector('[data-field="Explanation"]').value = card.explanation || "";
        var optionInputs = cardEl.querySelectorAll('[data-field="Option"]');
        (card.options || []).forEach(function (o, i) {
            if (optionInputs[i]) optionInputs[i].value = o.text || "";
        });
    }

    listEl.addEventListener("click", function (e) {
        var button = e.target.closest("[data-action]");
        if (!button) return;
        var card = button.closest(".question-card");
        var id = card.getAttribute("data-id");
        var action = button.getAttribute("data-action");

        if (action === "save") {
            window.SlideMaker.postForm(baseUrl + "Update", collectCardData(card)).then(function (resp) {
                if (resp.ok) window.SlideMaker.showToast("Question saved.");
                else window.SlideMaker.showToast("Could not save question.", "danger");
            });
        } else if (action === "delete") {
            if (!confirm("Delete this question?")) return;
            window.SlideMaker.postForm(baseUrl + "Delete", { id: id }).then(function (resp) {
                if (resp.ok) { card.remove(); renumber(); }
            });
        } else if (action === "duplicate" || action === "split") {
            var endpoint = action === "duplicate" ? "Duplicate" : "Split";
            window.SlideMaker.postForm(baseUrl + endpoint, { id: id }).then(function (r) { return r.json(); }).then(function (data) {
                var newCard = buildCardElement(data.card);
                card.after(newCard);
                renumber();
            });
        } else if (action === "merge") {
            var next = card.nextElementSibling;
            if (!next || !next.classList.contains("question-card")) {
                window.SlideMaker.showToast("There is no next question to merge with.", "danger");
                return;
            }
            var nextId = next.getAttribute("data-id");
            window.SlideMaker.postForm(baseUrl + "Merge", { id: id, nextId: nextId }).then(function (r) { return r.json(); }).then(function (data) {
                updateCardFields(card, data.card);
                next.remove();
                renumber();
            });
        } else if (action === "regenerate") {
            button.disabled = true;
            window.SlideMaker.postForm(baseUrl + "Regenerate", { id: id }).then(function (r) {
                return r.json().then(function (data) { return { ok: r.ok, data: data }; });
            }).then(function (result) {
                button.disabled = false;
                if (!result.ok) {
                    window.SlideMaker.showToast(result.data.message || "Could not regenerate.", "danger");
                    return;
                }
                updateCardFields(card, result.data.card);
                window.SlideMaker.showToast("Question regenerated.");
            });
        } else if (action === "move-up" || action === "move-down") {
            var sibling = action === "move-up" ? card.previousElementSibling : card.nextElementSibling;
            if (!sibling || !sibling.classList.contains("question-card")) return;
            if (action === "move-up") listEl.insertBefore(card, sibling);
            else listEl.insertBefore(sibling, card);
            renumber();
            persistOrder();
        }
    });

    function persistOrder() {
        var ids = Array.from(listEl.querySelectorAll(".question-card")).map(function (c) { return parseInt(c.getAttribute("data-id"), 10); });
        window.SlideMaker.postJson(baseUrl + "Reorder", ids);
    }

    document.getElementById("addQuestionBtn").addEventListener("click", function () {
        window.SlideMaker.postForm(baseUrl + "Add", {}).then(function (r) { return r.json(); }).then(function (data) {
            listEl.appendChild(buildCardElement(data.card));
            renumber();
        });
    });

    document.getElementById("continueBtn").addEventListener("click", function () {
        var ids = Array.from(listEl.querySelectorAll(".question-card")).map(function (c) { return c.getAttribute("data-id"); });
        if (ids.length === 0) {
            window.SlideMaker.showToast("Add at least one question before continuing.", "danger");
            return;
        }
        persistOrder();
        window.location.href = "/SlideMaker/Presentation/Pattern?presentationId=" + presentationId + "&ids=" + ids.join(",");
    });

    renumber();
})();
