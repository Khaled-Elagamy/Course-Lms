import 'https://cdnjs.cloudflare.com/ajax/libs/jquery-toast-plugin/1.3.2/jquery.toast.min.js';

function toggleEditability(card, isEditing) {
    card.find('.card-text').prop('contenteditable', isEditing);
    var cardInput = card.find('.form-control'); // Assuming the input field has the 'form-control' class
    cardInput.prop('disabled', !isEditing);
}

function updateCompletionText() {
    let courseId = $("#courseId").val();
    $.ajax({
        url: '/Courses/Api/UpdateCompletionText',
        type: 'POST',
        data: { courseId: courseId },
        success: function (result) {
            $('.completionText').html(result);
        },
        error: function (error) {
            console.error(error);
        }
    })
};

$(document).ready(function () {
    updateCompletionText();
    $(".addChapterBtn").on('click', function () {
        // Display the new chapter form
        $("#newChapterForm").toggle();
    });
    // Handler for input field change to enable/disable the "Create" button
    $("#newChapterTitle").on("input", function () {
        var createButton = $("#createChapterBtn");
        createButton.prop("disabled", $(this).val().trim() === "");
    });
    // Handler for form submission
    $("#newChapterForm").on('submit', function (e) {
        e.preventDefault();
        let courseId = $("#courseId").val();
        let newChapterTitle = $("#newChapterTitle").val();
        // Perform AJAX form submission
        $.ajax({
            url: $(this).attr("action"),
            method: "POST",
            data: {
                courseId: courseId,
                Title: newChapterTitle
            },
            success: function (data) {
                // Replace the content of the chapters container with the updated content
                $("#sortableChapters").html(data);
                // Reset and hide the new chapter form
                $("#newChapterTitle").val("");
                $("#createChapterBtn").prop("disabled", true);
                $("#newChapterForm").hide();
                $.toast({
                    text: 'Chapter Created',
                    position: 'top-center',
                    icon: 'success'
                })
                updateCompletionText();
            },
            error: function () {
                // Handle error
            }
        });
    });
    $('.imageInput').hide(); // Hide the <p> element by default
    let originalText;
    let courseId = $("#courseId").val();

    $(function () {
        $("#sortableChapters").sortable({
            update: function (event, ui) {
                let chapterOrder = $(this).sortable('toArray', { attribute: 'data-chapter-id' });
                chapterOrder = chapterOrder
                    .filter(function (position) {
                        return position !== '0' && position.trim() !== '';
                    })
                    .map(Number)
                if (chapterOrder.length > 0) {
                    $.ajax({
                        url: '/Courses/Api/UpdateChapterOrder',
                        type: 'POST',
                        data: { courseId: courseId, chapterOrder: chapterOrder },
                        success: function (response) {
                            if (response.success) {
                                $('#sortableChapters').load('/Courses/Api/GetChaptersData?courseId=' + courseId);
                                $.toast({
                                    text: 'Course Updated',
                                    position: 'top-center',
                                    icon: 'success'
                                })
                            } else {
                                $.toast({
                                    text: 'Failed to update chapter order.',
                                    position: 'top-center',
                                    icon: 'error'
                                })
                            }
                        },
                        error: function () {
                            $.toast({
                                text: 'Error during chapter order update.',
                                position: 'top-center',
                                icon: 'error'
                            })
                        }
                    });
                } else {
                    $.toast({
                        text: 'No valid chapter order data to send.',
                        position: 'top-center',
                        icon: 'error'
                    })
                }
            }
        });
        $("#sortableChapters").disableSelection();
    });
    $('.category-edit-button').on('click', function () {
        var card = $(this).closest('.card');
        var isEditing = card.hasClass('editing');

        if (isEditing) {
            // If currently editing, cancel editing
            categorytoggleEditability(card, false);
            card.removeClass('editing');
            card.find('.category-edit-button').html('<i class="bi bi-pencil"></i> Edit');
            card.find('.category-save-button').hide();
        } else {
            // If not editing, start editing
            categorytoggleEditability(card, true);
            card.addClass('editing');
            card.find('.category-edit-button').html('<i class="bi bi-x"></i> Cancel');
            card.find('.category-save-button').show();
        }
    });
    function categorytoggleEditability(card, isEditing) {
        card.find('.form-select').prop('disabled', !isEditing); // Disable/enable the select element
    }

    $('.category-save-button').on('click', function () {
        var card = $(this).closest('.card');
        var courseId = document.querySelector('.id').value;
        var updatedCategoryId = card.find('.form-select option:selected').val();

        // Check if updatedCategoryId is undefined or empty
        if (updatedCategoryId === undefined || updatedCategoryId === '') {
            // Handle the absence of a selected value (e.g., show an alert or provide a default value)
            console.log('No category selected. Provide a default value or handle it accordingly.');
            card.removeClass('editing');
            card.find('.category-edit-button').html('<i class="bi bi-pencil"></i> Edit');
            card.find('.category-save-button').hide();
            card.find('.form-select').prop('disabled', true); // Disable/enable the select element

            updateCompletionText();
            return; // Stop further execution
        }
        // Your AJAX request to update the category
        $.ajax({
            url: '/Courses/Api/UpdateCourseProperty',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({
                entityId: courseId,
                propertyName: "CategoryId",
                newValue: updatedCategoryId.toString(),
            }),
            success: function (response) {
                if (response.success) {
                    // Update the UI with the new value received from the server
                    card.find('.form-select').val(updatedCategoryId);
                    card.removeClass('editing');
                    card.find('.category-edit-button').html('<i class="bi bi-pencil"></i> Edit');
                    card.find('.category-save-button').hide();
                    card.find('.form-select').prop('disabled', true); // Disable/enable the select element
                    $.toast({
                        text: 'Course Updated',
                        position: 'top-center',
                        icon: 'success'
                    })
                    updateCompletionText();
                } else {
                    $.toast({
                        text: 'Failed to save data.',
                        position: 'top-center',
                        icon: 'error'
                    })
                    console.error(response.error + 'Failed to save data.');
                }
            },
            error: function (response) {
                console.log('Error during data save request.' + response);
            }
        });
    });
    $('.edit-button').on('click', function () {
        var card = $(this).closest('.card');
        var isEditing = card.hasClass('editing');
        var cardText = card.find('.card-text');

        if (isEditing) {
            // If currently editing, cancel editing
            cardText.text(originalText);
            toggleEditability(card, false);
            card.removeClass('editing');
            card.find('.edit-button').html('<i class="bi bi-pencil"></i> Edit');
            card.find('.save-button').hide();
            $("#titleErrorMessage").text("");
        } else {
            // If not editing, start editing
            toggleEditability(card, true);
            originalText = cardText.text();
            card.addClass('editing');
            card.find('.edit-button').html('<i class="bi bi-x"></i> Cancel');
            card.find('.save-button').show();
        }
    });
    $('.save-button').on('click', function (e) {
        e.preventDefault(); // Prevent the default form submission

        var card = $(this).closest('.card');
        var cardText = card.find('.card-text');
        var cardTitle = card.find('.card-title');
        var propertyName = cardTitle.text() // Retrieve property name dynamically
        var courseId = document.querySelector('.id').value;
        var propertyValue = cardText.length > 0 ? cardText.text() : card.find('.price-input').val().toString()

        $.ajax({
            url: '/Courses/Api/UpdateCourseProperty',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({
                entityId: courseId,
                propertyName: propertyName,
                newValue: propertyValue
            }),
            success: function (response) {
                if (response.success) {
                    // Update the UI with the new value received from the server
                    cardText.length > 0 ? cardText.text(propertyValue) : card.find('.price-input').value = propertyValue;
                    toggleEditability(card, false);
                    card.removeClass('editing');
                    card.find('.edit-button').html('<i class="bi bi-pencil"></i> Edit');
                    card.find('.save-button').hide();
                    $("#titleErrorMessage").text("");
                    originalText = cardText.text();
                    updateCompletionText();
                    $.toast({
                        text: 'Course Updated',
                        position: 'top-center',
                        icon: 'success'
                    })
                }
                else if (response.exists) {
                    $("#titleErrorMessage").text("A course with this title already exists.");
                } else {
                    $.toast({
                        text: 'Failed to save data.',
                        position: 'top-center',
                        icon: 'error'
                    })
                    console.error(response.error + 'Failed to save data.');
                    $("#titleErrorMessage").text("");
                }
                console.log(response);
            },
            error: function () {
                console.error('Error during data save request.');
            }
        });
    });

    $('#editImageButton').on('click', function () {
        var card = $(this).closest('.card');
        var isEditing = card.hasClass('editing');
        var image = $('#courseImage');
        var imageInput = $('.imageInput');


        if (isEditing) {
            // If currently editing, cancel editing
            toggleEditability(card, false);
            card.removeClass('editing');
            card.find('.edit-button').html('<i class="bi bi-pencil"></i> Edit');
        } else {
            // If not editing, start editing
            toggleEditability(card, true);
            card.addClass('editing');
            card.find('.edit-button').html('<i class="bi bi-x"></i> Cancel');
        }
        // Toggle visibility of image and file input
        image.toggle();
        imageInput.toggle();
    });
    $('.imageInput').on('change', function () {
        var input = this;
        var file = input.files[0];
        if (!file.type.startsWith('image/')) {
            // Show an error message
            Swal.fire('Error', 'Please select a valid image file.', 'error');
            // Clear the file input
            input.value = '';
            return;
        }

        var formData = new FormData($('#uploadForm')[0]);
        var courseId = document.querySelector('.id').value;
        formData.append('CourseId', courseId); // Append the CourseId to the FormData

        $.ajax({
            url: '/Courses/Api/UploadImage',
            type: 'POST',
            data: formData,
            contentType: false,
            processData: false,
            success: function (response) {
                if (response.success) {
                    Swal.fire('Success', 'Image uploaded successfully!', 'success');
                    var newImageUrl = "/images/" + response.image; // Assuming your response contains the new image file name
                    $('#courseImage').attr('src', newImageUrl);
                    $("#editImageButton").click();
                    updateCompletionText();
                } else {
                    // Handle failure (e.g., show an error message)
                    Swal.fire('Error', 'Failed to upload image. Please try again.', 'error');
                }
            },
            error: function () {
                // Handle error (e.g., show a generic error message)
                Swal.fire('Error', 'An error occurred during the upload process. Please try again.', 'error');
            }
        });
    });
});
function handleDelete() {
    let courseId = $("#courseId").val();
    Swal.fire({
        title: "Are you sure?",
        text: "You won't be able to revert this!",
        width: 600,
        padding: "1em",
        buttonsStyling: false,
        reverseButtons: true,
        showCancelButton: true,
        confirmButtonText: "Continue",
        customClass: {
            htmlContainer: 'swal-html-left',         // Custom class for the header
            title: 'swal-title-left',           // Custom class for the title
            confirmButton: 'btn btn-dark swal-confirm-btn', // Custom class for the delete button
            cancelButton: 'btn',
            actions: 'custom-actions',
            // Custom class for the cancel button
        }
    }).then((result) => {
        if (result.isConfirmed) {
            // User confirmed, make the AJAX call
            $.ajax({
                url: '/Courses/Api/DeleteCourse',
                type: 'POST',
                data: { courseId: courseId },
                success: function (result) {
                    // Handle the result, you can check success and show messages accordingly
                    if (result.success) {
                        Swal.fire({
                            title: "Deleted!",
                            text: "The Course has been deleted.",
                            icon: "success"
                        }).then((confirm) => {
                            if (confirm.isConfirmed || confirm.isDismissed) {
                                window.location.href = result.redirectUrl;
                            }
                        });
                    } else {
                        Swal.fire({
                            title: "Error!",
                            text: "Course deletion failed: " + result.message,
                            icon: "error"
                        });
                    }
                },
                error: function () {
                    Swal.fire({
                        title: "Error!",
                        text: "Error during Course deletion",
                        icon: "error"
                    });
                }
            });
        }
    });
};
function handlePublish(action) {
    let courseId = $("#courseId").val();
    console.log('test' + action)
    $.ajax({
        url: `/Courses/Api/${action}`,
        type: 'POST', // adjust the HTTP method as needed
        data: { courseId: courseId }, // include chapter ID in the request data
        success: function (response) {
            if (response.success) {
                $.toast({
                    text: response.message,
                    position: 'top-center',
                    icon: 'success'
                })
                updateCompletionText();
            }
            else {
                console.error(response.error + 'Failed to save data.');
                $.toast({
                    text: `Failed to Update\n${response.error}.`,
                    position: 'top-center',
                    icon: 'error'
                })
                updateCompletionText();
            }
        },
        error: function () {
            $.toast({
                heading: 'ServerError',
                text: 'Error during data save request.',
                position: 'top-center',
                icon: 'error'
            })
            updateCompletionText();
        }
    });
};
window.handlePublish = handlePublish;
window.handleDelete = handleDelete;