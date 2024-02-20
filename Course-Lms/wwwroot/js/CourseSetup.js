import 'https://cdnjs.cloudflare.com/ajax/libs/jquery-toast-plugin/1.3.2/jquery.toast.min.js';

function setupEditableForm(formSelector, propertyName, successCallback) {
    $(document).ready(function () {
        let originalText = {}; // Object to store original text for each field
        $(formSelector).on('submit', function (e) {
            e.preventDefault();

            let card = $(formSelector + ' .edit-button').closest('.card');
            let cardText = $(formSelector).find('.form-control');
            let courseId = document.querySelector('.id').value;
            let propertyValue = cardText.val();

            //Check if the value has changed
            if (propertyValue === originalText[formSelector]) {
                // No change, do not make the AJAX call
                toggleEditability(card, false);
                return; // Exit early
            }
            //Todo Handel course title error
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
                        cardText.val(propertyValue);
                        toggleEditability(card, false);
                        if (successCallback && typeof successCallback === 'function') {
                            successCallback(response);
                        }
                        updateCompletionText();
                        $.toast({
                            text: 'Course Updated',
                            position: 'top-center',
                            icon: 'success'
                        })
                    } else if (response.exists) {
                        $("#titleErrorMessage").text("A course with this title already exists.");
                    } else {
                        console.error(response.error + 'Failed to save data.');
                        $.toast({
                            text: 'Failed to save data.',
                            position: 'top-center',
                            icon: 'error'
                        })
                    }
                },
                error: function () {
                    console.error('Error during data save request.');
                }
            });
        });
        $(formSelector + ' .edit-button').on('click', function () {
            let card = $(this).closest('.card');
            let isEditing = card.hasClass('editing');
            let cardText = card.find('.form-control');
            if (isEditing) {
                cardText.val(originalText[formSelector]);
                toggleEditability(card, false);
            } else {
                toggleEditability(card, true);
                originalText[formSelector] = cardText.val();
            }
        });
        function toggleEditability(card, isEditing) {
            card.find('.form-control').prop('disabled', !isEditing);
            if (isEditing) {
                card.addClass('editing');
                $(formSelector + ' .edit-button').html('<i class="bi bi-x icon"></i> Cancel ');
                $(formSelector + ' .save-button').show();
            } else {
                $(formSelector + ' .edit-button').html('<i class="bi bi-pencil icon"></i> Edit ' + propertyName.toLowerCase());
                $(formSelector + ' .save-button').hide();
                card.removeClass('editing');
                if (propertyName == "Title") {
                    $("#titleErrorMessage").text("");
                };
                if (propertyName == "Description") {
                    $("textarea").each(function () {
                        resizeTextArea.call(this);
                    });
                }
            }
        }
    });
}
function setupEditableInput(formSelector, propertyName) {
    $(document).ready(function () {
        let originalData = {}; // Object to store original text for each field
        $(`#${propertyName}-save-button`).on('click', function (e) {
            e.preventDefault();

            let card = $(`#${propertyName}-edit-button`).closest('.card');
            let cardText = $(formSelector).find('.form-control');
            let courseId = document.querySelector('.id').value;
            let propertyValue = cardText.val();

            if (propertyName == "Category") {
                propertyValue = $('.selectpicker').val();
            }

            // Check if updatedCategoryId is undefined or empty
            if (propertyValue === undefined || propertyValue === '') {
                $.toast({
                    text: 'Choose a category',
                    position: 'top-center',
                    icon: 'error'
                })
                updateCompletionText();
                return;
            }

            //Check if the value has changed
            if (propertyValue === originalData[formSelector]) {
                toggleEditability(card, false);
                return;
            }

            //Todo Handel course title error
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
                        if (propertyName == "Category") {
                            let selectedName = $('.selectpicker option:selected').text();
                            $(`#${propertyName}Text`).text(selectedName);
                        } else {
                            $(`#${propertyName}Text`).text("$" + propertyValue);
                        }

                        toggleEditability(card, false);

                        updateCompletionText();
                        $.toast({
                            text: 'Course Updated',
                            position: 'top-center',
                            icon: 'success'
                        })
                    } else {
                        console.error(response.error + 'Failed to save data.');
                        $.toast({
                            text: 'Failed to save data.',
                            position: 'top-center',
                            icon: 'error'
                        })
                    }
                },
                error: function () {
                    console.error('Error during data save request.');
                }
            });
        });
        $(`#${propertyName}-edit-button`).on('click', function () {
            let card = $(this).closest('.card');
            let isEditing = card.hasClass('editing');
            let cardText = card.find('.form-control');
            if (isEditing) {
                if (propertyName == "Category") {
                    card.find('.selectpicker').val(originalData[formSelector]);
                } else {
                    cardText.val(originalData[formSelector]);
                }
                toggleEditability(card, false);
            } else {
                toggleEditability(card, true);
                if (propertyName == "Category") {
                    originalData[formSelector] = $('.selectpicker').val();
                } else {
                    originalData[formSelector] = cardText.val();
                }
            }
        });
        function toggleEditability(card, isEditing) {
            card.find('.form-control').prop('disabled', !isEditing);
            if (isEditing) {
                card.addClass('editing');
                $(formSelector).show();
                if (propertyName == "Category") {
                    $("#CategoryId").attr('disabled', false);
                    $('#CategoryId').selectpicker('refresh');
                }
                $(`#${propertyName}Text`).hide()
                $(`#${propertyName}-edit-button`).html('<i class="bi bi-x icon"></i> Cancel ');
                $(`#${propertyName}-save-button`).show();
            } else {
                $(formSelector).hide();
                if (propertyName == "Category") {
                    $("#CategoryId").attr('disabled', true);
                    $('#CategoryId').selectpicker('refresh');
                }
                $(`#${propertyName}Text`).show()
                $(`#${propertyName}-edit-button`).html('<i class="bi bi-pencil icon"></i> Edit ' + propertyName.toLowerCase());
                $(`#${propertyName}-save-button`).hide();
                card.removeClass('editing');
            }
        }
    });
}
$("textarea").each(function () {
    this.setAttribute("style", "height:" + (this.scrollHeight) + "px;overflow-y:hidden;");
}).on("input paste", resizeTextArea);
function resizeTextArea() {
    this.style.height = 0;
    this.style.height = (this.scrollHeight) + "px";
}

setupEditableForm('#titleForm', 'Title', () => {
    $("#titleErrorMessage").text("");
});
setupEditableForm('#descriptionForm', 'Description', "");

setupEditableInput('#categoryForm', 'Category');
setupEditableInput('#priceForm', 'Price');

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
    $('.loader').hide();

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
    let courseId = $(".id").val();

    $(function () {
        $("#sortableChapters").sortable({
            handle: '.chapter-grip-icon',
            connectWith: ".chapter-card",
            update: function (event, ui) {
                let chapterOrder = $(this).sortable('toArray', { attribute: 'data-chapter-id' });
                chapterOrder = chapterOrder
                    .filter(function (position) {
                        return position !== '0' && position.trim() !== '';
                    })
                    .map(Number)
                if (chapterOrder.length > 0) {
                    $('.loader').show();
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
                            $('.loader').hide();
                        },
                        error: function () {
                            $.toast({
                                text: 'Error during chapter order update.',
                                position: 'top-center',
                                icon: 'error'
                            })
                            $('.loader').hide();
                        }
                    });
                } else {
                    $.toast({
                        text: 'No valid chapter order data to send.',
                        position: 'top-center',
                        icon: 'error'
                    })
                    $('.loader').hide();
                }
            }
        });
        $("#sortableChapters").disableSelection();
    });
    $('.image-edit-button').on('click', function () {
        let card = $('.image-card');
        let isEditing = card.hasClass('editing');
        if (isEditing) {
            toggleImageEditability(false);
        } else {
            toggleImageEditability(true);
        }
    });
});
function toggleImageEditability(isEditing) {
    let card = $('.image-card');
    if (isEditing) {
        card.addClass('editing');
        $('.image-edit-button').html('<i class="bi bi-x icon"></i> Cancel ');
        $('#imageUploadFormContainer').show();
        $('#image-container').hide();
    } else {
        $('.image-edit-button').html('<i class="bi bi-pencil icon"></i> Edit image');
        $('#image-container').show();
        $('#imageUploadFormContainer').hide();
        card.removeClass('editing');
    }
}
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
window.toggleImageEditability = toggleImageEditability;
window.handlePublish = handlePublish;
window.handleDelete = handleDelete;