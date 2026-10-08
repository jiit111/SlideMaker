(function () {
    "use strict";

    var dropZone = document.getElementById("dropZone");
    var fileInput = document.getElementById("fileInput");
    var form = document.getElementById("uploadForm");
    var progressWrap = document.getElementById("uploadProgressWrap");
    var progressBar = document.getElementById("uploadProgressBar");
    var submitBtn = document.getElementById("uploadSubmitBtn");

    if (!form) return;

    dropZone.addEventListener("click", function (e) {
        if (e.target !== fileInput) fileInput.click();
    });

    ["dragover", "dragenter"].forEach(function (evt) {
        dropZone.addEventListener(evt, function (e) {
            e.preventDefault();
            dropZone.classList.add("border-primary");
        });
    });
    ["dragleave", "drop"].forEach(function (evt) {
        dropZone.addEventListener(evt, function (e) {
            e.preventDefault();
            dropZone.classList.remove("border-primary");
        });
    });
    dropZone.addEventListener("drop", function (e) {
        if (e.dataTransfer.files.length > 0) {
            fileInput.files = e.dataTransfer.files;
        }
    });

    form.addEventListener("submit", function (e) {
        e.preventDefault();
        if (fileInput.files.length === 0) {
            window.SlideMaker.showToast("Please choose at least one image.", "danger");
            return;
        }

        var formData = new FormData(form);
        var xhr = new XMLHttpRequest();
        xhr.open("POST", form.getAttribute("action") || window.location.href);

        progressWrap.classList.remove("d-none");
        submitBtn.disabled = true;

        xhr.upload.addEventListener("progress", function (evt) {
            if (evt.lengthComputable) {
                var pct = Math.round((evt.loaded / evt.total) * 100);
                progressBar.style.width = pct + "%";
                progressBar.textContent = pct + "%";
            }
        });

        xhr.addEventListener("load", function () {
            if (xhr.status >= 200 && xhr.status < 400) {
                window.location.href = xhr.responseURL || window.location.href;
            } else {
                submitBtn.disabled = false;
                window.SlideMaker.showToast("Upload failed. Please try again.", "danger");
            }
        });

        xhr.addEventListener("error", function () {
            submitBtn.disabled = false;
            window.SlideMaker.showToast("Upload failed. Please try again.", "danger");
        });

        xhr.send(formData);
    });
})();
