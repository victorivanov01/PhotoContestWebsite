//// Save the scroll position before the page unloads (refresh or navigation)
//window.addEventListener('beforeunload', function() {
//    localStorage.setItem('scrollPosition', window.scrollY);
//});

//// Restore the scroll position before the page is fully rendered
//window.addEventListener('load', function() {
//    const savedPosition = localStorage.getItem('scrollPosition');
//    if (savedPosition) {
//        // Apply scroll position instantly without causing any jump
//        document.documentElement.scrollTop = savedPosition;  // For modern browsers
//        document.body.scrollTop = savedPosition;  // For older browsers
//    }
//});
document.addEventListener("DOMContentLoaded", function () {
    const likeButton = document.getElementById("likeButton");
    const voteMessage = document.getElementById("voteMessage");
    const likeCountElement = document.getElementById("likeCount");
    const pictureIdInput = document.getElementById("pictureId");

    // Check contest status
    fetch("/Admin/GetContestStatusJson")
        .then(response => response.json())
        .then(data => {
            if (!data.isActive) {
                likeButton.disabled = true;
                voteMessage.textContent = "The contest has ended. Voting is disabled.";
                voteMessage.className = "error"; // Use your existing error class
                voteMessage.style.display = "block"; // Show the message
            } else {
                likeButton.disabled = false;
                voteMessage.style.display = "none"; // Hide the message when active
            }
        });

    if (likeButton) {
        likeButton.addEventListener("click", async function (event) {
            event.preventDefault();

            if (!pictureIdInput) {
                console.error("Hidden input for picture ID not found!");
                return;
            }

            const pictureId = pictureIdInput.value;

            try {
                const response = await fetch(`/Home/LikePicture`, {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json",
                    },
                    body: JSON.stringify(parseInt(pictureId)),
                });

                const data = await response.json();

                if (data.success) {
                    likeCountElement.textContent = `Likes: ${data.likes}`;
                    voteMessage.textContent = data.message || "Thank you for voting!";
                    voteMessage.className = "success"; // Use your existing success class
                    voteMessage.style.display = "block";
                    likeButton.disabled = true;
                } else {
                    voteMessage.textContent = data.message || "You have already voted.";
                    voteMessage.className = "error"; // Use your existing error class
                    voteMessage.style.display = "block";
                }

                setTimeout(() => {
                    voteMessage.style.display = "none";
                }, 3000); // Hide after 3 seconds
            } catch (error) {
                console.error("Error:", error);
                voteMessage.textContent = "An error occurred while voting.";
                voteMessage.className = "error"; // Use your existing error class
                voteMessage.style.display = "block";

                setTimeout(() => {
                    voteMessage.style.display = "none";
                }, 3000); // Hide after 3 seconds
            }
        });
    }
});


document.addEventListener("DOMContentLoaded", function () {
    const imageInput = document.getElementById("imageFile");
    const imagePreview = document.getElementById("imagePreview");
    const imagePreviewContainer = document.getElementById("imagePreviewContainer");
    const errorMessage = document.getElementById("errorMessage");
    const form = document.getElementById("submissionForm");
    const submitButton = form.querySelector("button[type='submit']");

    // Allowed file types and maximum file size (25MB)
    const allowedFileTypes = ["image/jpeg", "image/png", "image/gif"];
    const maxFileSize = 25 * 1024 * 1024; // 25MB

    // Handle file input change
    imageInput.addEventListener("change", function () {
        const file = imageInput.files[0];

        // Reset error message and preview
        errorMessage.style.display = "none";
        imagePreviewContainer.style.display = "none";

        if (file) {
            // Validate file type
            if (!allowedFileTypes.includes(file.type)) {
                errorMessage.textContent = "Invalid file type. Please upload a JPEG, PNG, or GIF image.";
                errorMessage.style.display = "block";
                imageInput.value = ""; // Clear the input
                return;
            }

            // Validate file size
            if (file.size > maxFileSize) {
                errorMessage.textContent = "File size exceeds 25MB. Please upload a smaller image.";
                errorMessage.style.display = "block";
                imageInput.value = ""; // Clear the input
                return;
            }

            // Show preview
            const reader = new FileReader();
            reader.onload = function (e) {
                imagePreview.src = e.target.result; // Set the preview image source
                imagePreviewContainer.style.display = "block";
            };
            reader.readAsDataURL(file);
        }
    });

    // Handle form submission using AJAX
    form.addEventListener("submit", function (event) {
        event.preventDefault(); // Prevent the default form submission

        const formData = new FormData(form); // Create FormData object

        fetch("/Home/SubmitPicture", {
            method: "POST",
            body: formData,
        })
            .then(response => {
                if (!response.ok) {
                    throw new Error("Network response was not ok");
                }
                return response.json();
            })
            .then(data => {
                if (data.success) {
                    // Display success message near the submit button
                    const successMessage = document.createElement("div");
                    successMessage.textContent = data.message || "Picture submitted successfully!";
                    successMessage.className = "custom-success-popup"; // Styled as a popup
                    successMessage.id = "successMessage";

                    // Remove any existing success message
                    const existingMessage = document.getElementById("successMessage");
                    if (existingMessage) {
                        existingMessage.remove();
                    }

                    // Append the message after the submit button
                    submitButton.parentElement.appendChild(successMessage);

                    // Reset form and preview
                    form.reset();
                    imagePreviewContainer.style.display = "none";

                    // Automatically fade out the message after 5 seconds
                    setTimeout(() => {
                        if (successMessage) {
                            successMessage.remove();
                        }
                    }, 5000);
                } else {
                    errorMessage.textContent = data.message || "An error occurred while submitting the picture.";
                    errorMessage.style.display = "block";
                }
            })
            .catch(error => {
                console.error("Error:", error);
                errorMessage.textContent = "An unexpected error occurred.";
                errorMessage.style.display = "block";
            });
    });
});


document.addEventListener("DOMContentLoaded", function () {
    const images = document.querySelectorAll(".clickable-image");  // Get all images with the clickable-image class
    const fullScreenContainer = document.getElementById("fullScreenContainer");
    const fullScreenImage = document.getElementById("fullScreenImage");
    const closeFullScreenButton = document.getElementById("closeFullScreen");

    // When an image is clicked, open it in fullscreen
    images.forEach(image => {
        image.addEventListener("click", function () {
            const src = this.getAttribute("data-src");  // Get the full-size image URL
            fullScreenImage.src = src;  // Set the full-screen image's source
            fullScreenContainer.style.display = "flex";  // Show the full-screen container
        });
    });

    // Close the full-screen image when the close button is clicked
    closeFullScreenButton.addEventListener("click", function () {
        fullScreenContainer.style.display = "none";  // Hide the full-screen container
    });

    // Close the full-screen view when clicking outside the image (on the container)
    fullScreenContainer.addEventListener("click", function (event) {
        if (event.target === fullScreenContainer) {
            fullScreenContainer.style.display = "none";  // Hide if clicked outside the image
        }
    });
});
