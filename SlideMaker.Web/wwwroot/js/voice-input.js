/*
 * Wires any [data-mic-for="<inputId>"] button to the browser's Web Speech API.
 * Speech is transcribed client-side and written into the target input/textarea.
 * No server-side speech-to-text yet (see Services/Voice/IVoiceService.cs) -- this
 * is intentionally the whole implementation for Phase 1.
 */
(function () {
    "use strict";

    var SpeechRecognitionImpl = window.SpeechRecognition || window.webkitSpeechRecognition;

    function attach(button) {
        var targetId = button.getAttribute("data-mic-for");
        var target = document.getElementById(targetId);
        if (!target) return;

        if (!SpeechRecognitionImpl) {
            button.disabled = true;
            button.title = "Voice input is not supported in this browser";
            return;
        }

        var recognition = new SpeechRecognitionImpl();
        recognition.lang = button.getAttribute("data-lang") || "en-IN";
        recognition.interimResults = false;
        recognition.maxAlternatives = 1;

        var listening = false;

        recognition.addEventListener("result", function (event) {
            var transcript = event.results[0][0].transcript;
            if (target.value && target.value.trim().length > 0) {
                target.value = target.value.trim() + " " + transcript;
            } else {
                target.value = transcript;
            }
            target.dispatchEvent(new Event("input", { bubbles: true }));
        });

        recognition.addEventListener("end", function () {
            listening = false;
            button.classList.remove("listening");
        });

        recognition.addEventListener("error", function () {
            listening = false;
            button.classList.remove("listening");
        });

        button.addEventListener("click", function () {
            if (listening) {
                recognition.stop();
                return;
            }
            listening = true;
            button.classList.add("listening");
            try {
                recognition.start();
            } catch (e) {
                listening = false;
                button.classList.remove("listening");
            }
        });
    }

    document.addEventListener("DOMContentLoaded", function () {
        document.querySelectorAll("[data-mic-for]").forEach(attach);
    });
})();
