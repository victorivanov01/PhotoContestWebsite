console.log("Admin panel script loaded!");

document.addEventListener("DOMContentLoaded", function () {


    // Helper function to send AJAX requests
    function toggleStatus(url, id, callback) {
        fetch(url, {
            method: "POST",
            headers: { "X-Requested-With": "XMLHttpRequest" },
        })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    callback(data);
                } else {
                    alert(data.message || "An error occurred.");
                }
            })
            .catch(error => console.error("Error:", error));
    }

    // Toggle Display
    document.querySelectorAll(".toggle-display").forEach(button => {
        button.addEventListener("click", function () {
            const pictureId = this.dataset.id;
            toggleStatus(`/Admin/ToggleDisplay/${pictureId}`, pictureId, (data) => {
                this.textContent = data.isDisplayed ? "Hide" : "Show";
            });
        });
    });

    // Toggle Winner
    document.querySelectorAll(".toggle-winner").forEach(button => {
        button.addEventListener("click", function () {
            const pictureId = this.dataset.id;
            toggleStatus(`/Admin/ToggleWinner/${pictureId}`, pictureId, (data) => {
                this.textContent = data.isWinner ? "Remove Winner" : "Set Winner";
            });
        });
    });

    // Edit Category
    document.querySelectorAll(".category-select").forEach(select => {
        select.addEventListener("change", function () {
            const pictureId = this.dataset.id;
            const newCategory = this.value;

            fetch(`/Admin/EditCategory`, {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ id: pictureId, newCategory: newCategory })
            })
                .then(response => response.json())
                .then(data => {
                    if (!data.success) {
                        alert("Error: " + data.message);
                    }
                });
        });
    });

    // Inline Editing Handlers
    document.querySelectorAll(".edit-btn").forEach(button => {
        button.addEventListener("click", function () {
            const row = this.closest("tr");
            row.querySelectorAll(".view-mode").forEach(el => el.style.display = "none");
            row.querySelectorAll(".edit-mode").forEach(el => el.style.display = "block");
            row.querySelector(".edit-btn").style.display = "none";
            row.querySelector(".save-btn").style.display = "inline-block";
            row.querySelector(".cancel-btn").style.display = "inline-block";
        });
    });
    document.querySelectorAll(".save-btn").forEach(button => {
        button.addEventListener("click", function () {
            const row = this.closest("tr"); // Find the row containing the button
            const id = row.getAttribute("data-id"); // Retrieve the ID from the row

            // Use specific selectors for editable fields
            const titleElement = row.querySelector("input[type='text'].edit-mode");
            const descriptionElement = row.querySelector("textarea.edit-mode");
            const categoryElement = row.querySelector("select.edit-mode");

            // Safely retrieve the values
            const title = titleElement ? titleElement.value : null;
            const description = descriptionElement ? descriptionElement.value : null;
            const category = categoryElement ? categoryElement.value : null;

            if (!title || !description || !category) {
                console.error("Missing values in editable fields:", { title, description, category });
                return; // Stop if any field is missing
            }

            // Log the data being sent
            console.log("Saving changes:", { id, title, description, category });

            // Send AJAX request to save changes
            fetch(`/Admin/EditPhoto`, {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify({ id, title, description, category }),
            })
                .then(response => response.json())
                .then(data => {
                    if (data.success) {
                        alert("Changes saved successfully!");

                        // Update the visible values in the current row
                        row.querySelector("td:nth-child(2) .view-mode").textContent = title;
                        row.querySelector("td:nth-child(3) .view-mode").textContent = description;
                        row.querySelector("td:nth-child(4) .view-mode").textContent = category;

                        // Hide edit mode and return to view mode
                        row.querySelectorAll(".view-mode").forEach(el => el.style.display = "block");
                        row.querySelectorAll(".edit-mode").forEach(el => el.style.display = "none");
                        row.querySelector(".edit-btn").style.display = "inline-block";
                        row.querySelector(".save-btn").style.display = "none";
                        row.querySelector(".cancel-btn").style.display = "none";

                        // Check if the category has changed
                        const currentCategoryHeader = row.closest("tbody").previousElementSibling;
                        if (!currentCategoryHeader || currentCategoryHeader.textContent.trim() !== category) {
                            // Move the row to the correct category section

                            // Find the target category section
                            const categoryHeaders = Array.from(document.querySelectorAll("thead + tbody"));
                            const targetCategoryBody = categoryHeaders.find(
                                body => body.previousElementSibling.textContent.trim() === category
                            );

                            if (targetCategoryBody) {
                                targetCategoryBody.appendChild(row); // Move the row to the correct section
                            } else {
                                console.error(`Category '${category}' not found.`);
                            }
                        }
                    } else {
                        alert("Error saving changes: " + data.message);
                    }
                })
                .catch(err => console.error("Error:", err));
        });
    });




    document.querySelectorAll(".cancel-btn").forEach(button => {
        button.addEventListener("click", function () {
            const row = this.closest("tr");
            row.querySelectorAll(".view-mode").forEach(el => el.style.display = "block");
            row.querySelectorAll(".edit-mode").forEach(el => el.style.display = "none");
            row.querySelector(".edit-btn").style.display = "inline-block";
            row.querySelector(".save-btn").style.display = "none";
            row.querySelector(".cancel-btn").style.display = "none";
        });
    });


    document.addEventListener("click", function (event) {
        if (event.target && event.target.classList.contains("delete-btn")) {
            const button = event.target;
            const row = button.closest("tr");
            const id = parseInt(row.getAttribute("data-id"));

            console.log(`Attempting to delete photo with ID: ${id}`); // Debugging log

            if (!confirm("Are you sure you want to delete this photo?")) {
                return; // Exit if user cancels
            }

            // Send DELETE request with ID in the URL
            fetch(`/Admin/DeletePhoto?id=${id}`, {
                method: "POST",
            })
                .then(response => response.json())
                .then(data => {
                    if (data.success) {
                        alert(data.message);

                        // Remove the row
                        row.remove();
                    } else {
                        alert("Error deleting photo: " + data.message);
                    }
                })
                .catch(err => console.error("Error:", err));
        }
    });

});
document.addEventListener("DOMContentLoaded", function () {
    console.log("Admin panel script loaded!");

    // References to the buttons
    const displayAllButton = document.getElementById("display-all");
    const hideAllButton = document.getElementById("hide-all");

    // Display All button functionality
    displayAllButton.addEventListener("click", function () {
        fetch(`/Admin/ToggleAllDisplays?display=true`, {
            method: "POST",
            headers: { "X-Requested-With": "XMLHttpRequest" },
        })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    // Update the Display status column in the table
                    document.querySelectorAll(".toggle-display").forEach(button => {
                        button.textContent = "Hide";
                    });

                    alert(data.message);
                } else {
                    alert(data.message || "An error occurred.");
                }
            })
            .catch(error => console.error("Error:", error));
    });

    // Hide All button functionality
    hideAllButton.addEventListener("click", function () {
        fetch(`/Admin/ToggleAllDisplays?display=false`, {
            method: "POST",
            headers: { "X-Requested-With": "XMLHttpRequest" },
        })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    // Update the Display status column in the table
                    document.querySelectorAll(".toggle-display").forEach(button => {
                        button.textContent = "Show";
                    });

                    alert(data.message);
                } else {
                    alert(data.message || "An error occurred.");
                }
            })
            .catch(error => console.error("Error:", error));
    });
});
document.addEventListener("DOMContentLoaded", function () {
    const setWinnersButton = document.getElementById("set-winners");

    setWinnersButton.addEventListener("click", function () {
        if (!confirm("Are you sure you want to set winners based on the most likes in each category?")) {
            return;
        }

        fetch("/Admin/SetWinnersByLikes", {
            method: "POST",
            headers: { "X-Requested-With": "XMLHttpRequest" },
        })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    alert(data.message);

                    // Optional: Refresh the page or update the table dynamically
                    location.reload(); // Refresh the page
                } else {
                    alert(data.message || "An error occurred.");
                }
            })
            .catch(error => console.error("Error:", error));
    });
});
document.addEventListener("DOMContentLoaded", function () {
    const removeAllWinnersButton = document.getElementById("remove-all-winners");

    removeAllWinnersButton.addEventListener("click", function () {
        // Send a POST request to remove all winners
        fetch(`/Admin/RemoveAllWinners`, {
            method: "POST",
            headers: { "X-Requested-With": "XMLHttpRequest" },
        })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    alert(data.message);

                    // Update the Winner status column dynamically
                    document.querySelectorAll("td.winner-status").forEach(cell => {
                        cell.textContent = "No"; // Update winner status to "No"
                    });

                    // Update any UI elements (e.g., buttons) related to winners dynamically
                    document.querySelectorAll(".toggle-winner").forEach(button => {
                        button.textContent = "Set Winner"; // Update button text
                    });
                } else {
                    alert(data.message || "An error occurred.");
                }
            })
            .catch(error => console.error("Error:", error));
    });
});

document.addEventListener("DOMContentLoaded", function () {
    const toggleContestButton = document.getElementById("toggleContestButton");
    const contestStatus = document.getElementById("contestStatus");

    toggleContestButton.addEventListener("click", function () {
        fetch('/Admin/ToggleContest', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'X-Requested-With': 'XMLHttpRequest'
            }
        })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    alert(data.message);
                    const isActive = data.message.includes("started");
                    toggleContestButton.textContent = isActive ? "End Contest" : "Start Contest";
                    contestStatus.textContent = isActive ? "Contest is active" : "Contest has ended";
                } else {
                    alert("Failed to update contest status.");
                }
            })
            .catch(error => console.error("Error toggling contest:", error));
    });
});

document.addEventListener("DOMContentLoaded", function () {
    const toggleSubmissionButton = document.getElementById("toggleSubmissionButton");
    const submissionStatus = document.getElementById("submissionStatus");

    // Fetch the initial status of submissions from the server when the page loads
    fetch("/Admin/GetSubmissionStatus")
        .then(response => response.json())
        .then(data => {
            updateSubmissionStatus(data.isActive);  // Update the page with the initial status
        })
        .catch(error => console.error("Error fetching submission status:", error));

    // Function to update the UI based on the submission status
    function updateSubmissionStatus(isActive) {
        if (isActive) {
            submissionStatus.textContent = "Submissions are active";
            toggleSubmissionButton.textContent = "End Submissions";
        } else {
            submissionStatus.textContent = "Submissions are closed";
            toggleSubmissionButton.textContent = "Start Submissions";
        }
    }

    // When the button is clicked, toggle the submission status and update the page dynamically
    toggleSubmissionButton.addEventListener("click", function () {
        fetch("/Admin/ToggleSubmissionStatus", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            }
        })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    // Update the UI based on the new status
                    updateSubmissionStatus(data.isActive);
                }
            })
            .catch(error => {
                console.error("Error toggling submission status:", error);
            });
    });

    const toggleContestButton = document.getElementById("toggleContestButton");
    const contestStatus = document.getElementById("contestStatus");

    // Fetch the initial status of the contest from the server when the page loads
    fetch("/Admin/GetContestStatusJson")
        .then(response => response.json())
        .then(data => {
            updateContestStatus(data.isActive);  // Update the page with the initial status
        })
        .catch(error => console.error("Error fetching contest status:", error));

    // Function to update the UI based on the contest status
    function updateContestStatus(isActive) {
        if (isActive) {
            contestStatus.textContent = "Contest is active";
            toggleContestButton.textContent = "End Contest";
        } else {
            contestStatus.textContent = "Contest has ended";
            toggleContestButton.textContent = "Start Contest";
        }
    }

    // When the button is clicked, toggle the contest status and update the page dynamically
    toggleContestButton.addEventListener("click", function () {
        fetch("/Admin/ToggleContest", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            }
        })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    // Update the UI based on the new status
                    updateContestStatus(data.isActive);
                }
            })
            .catch(error => {
                console.error("Error toggling contest status:", error);
            });
    });
});
document.addEventListener("DOMContentLoaded", function () {
    const images = document.querySelectorAll(".clickable-image"); // Select all images that should open in full-screen
    const fullScreenContainer = document.getElementById("fullScreenContainer");
    const fullScreenImage = document.getElementById("fullScreenImage");
    const closeFullScreenButton = document.getElementById("closeFullScreen");

    // When an image is clicked, open it in fullscreen
    images.forEach(image => {
        image.addEventListener("click", function () {
            const src = this.getAttribute("data-src"); // Get the image source from the data-src attribute
            fullScreenImage.src = src; // Set the full-screen image source
            fullScreenContainer.style.display = "flex"; // Show the full-screen container
        });
    });

    // When the close button is clicked, hide the full-screen view
    closeFullScreenButton.addEventListener("click", function () {
        fullScreenContainer.style.display = "none"; // Hide the full-screen container
    });

    // Optionally: Close the full-screen view if the background is clicked (outside the image)
    fullScreenContainer.addEventListener("click", function (event) {
        if (event.target === fullScreenContainer) {
            fullScreenContainer.style.display = "none"; // Hide if clicked outside the image
        }
    });
});
