import 'https://cdnjs.cloudflare.com/ajax/libs/jquery-toast-plugin/1.3.2/jquery.toast.min.js';

function setupEditableForm(formSelector, propertyName, successCallback) {
    $(document).ready(function () {
        let originalText = {}; // Object to store original text for each field
        $(formSelector).on('submit', function (e) {
            e.preventDefault();

            let card = $(formSelector + ' .edit-button').closest('.card');
            let cardText = $(formSelector).find('.form-control');
            let chapterId = document.querySelector('.id').value;
            let propertyValue = cardText.val();
            //Todo Handel course title error
            $.ajax({
                url: '/Chapters/UpdateChapterProperty',
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify({
                    entityId: chapterId,
                    propertyName: propertyName,
                    newValue: propertyValue
                }),
                success: function (response) {
                    if (response.success) {
                        cardText.val(propertyValue);
                        toggleEditability(card, false);
                        if (successCallback && typeof successCallback === 'function') {
                            successCallback(response);
                            updateCompletionText();
                            $.toast({
                                text: 'Chapter Updated',
                                position: 'top-center',
                                icon: 'success'
                            })
                        }
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
                $(formSelector + ' .edit-button').html('<i class="bi bi-pencil icon"></i> Edit ' + propertyName);
                $(formSelector + ' .save-button').hide();
                card.removeClass('editing');
            }
        }
    });
}
$("textarea").each(function () {
    this.setAttribute("style", "height:" + (this.scrollHeight) + "px;overflow-y:hidden;");
}).on("input", function () {
    this.style.height = 0;
    this.style.height = (this.scrollHeight) + "px";
});
function updateCompletionText() {
    let chapterId = document.querySelector('.id').value;
    $.ajax({
        url: '/Chapters/UpdateCompletionText',
        type: 'POST',
        data: { chapterId: chapterId },
        success: function (result) {
            $('.completionText').html(result);
        },
        error: function (error) {
            console.error(error);
        }
    })
};

setupEditableForm('#titleForm', 'Title',"");

setupEditableForm('#descriptionForm', 'Description', "");

//Publish Action
function handleDelete() {
    let chapterId = document.querySelector('.id').value;
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
            htmlContainer: 'swal-html-left',
            title: 'swal-title-left',
            confirmButton: 'btn btn-dark swal-confirm-btn',
            cancelButton: 'btn',
            actions: 'custom-actions',
        }
    }).then((result) => {
        if (result.isConfirmed) {
            // User confirmed, make the AJAX call
            $.ajax({
                url: '/Chapters/DeleteChapter',
                type: 'POST',
                data: { chapterId: chapterId },
                success: function (result) {

                    if (result.success) {
                        Swal.fire({
                            title: "Deleted!",
                            text: "The chapter has been deleted.",
                            icon: "success"
                        }).then((confirm) => {
                            if (confirm.isConfirmed || confirm.isDismissed) {
                                window.location.href = result.redirectUrl;
                            }
                        });
                    } else {
                        Swal.fire({
                            title: "Error!",
                            text: "Chapter deletion failed: " + result.message,
                            icon: "error"
                        });
                    }
                },
                error: function () {
                    Swal.fire({
                        title: "Error!",
                        text: "Error during chapter deletion",
                        icon: "error"
                    });
                }
            });
        }
    });
};
function handlePublish(action) {
    let chapterId = document.querySelector('.id').value;
    console.log('test' + action)
    $.ajax({
        url: `/Chapters/${action}`,
        type: 'POST', // adjust the HTTP method as needed
        data: { chapterId: chapterId }, // include chapter ID in the request data
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
function reloadVideoPartial(chapterId, filename) {
    $('#video-container').load('/Chapters/VideoPlayer?chapterId=' + chapterId + '&filename=' + filename);
    updateCompletionText();
}
$(document).ready(function () {
    let originalCheckboxState = false;

    updateCompletionText();

    $('.access-edit-button').on('click', function () {
        let form = $('#accessForm');
        let isEditing = form.hasClass('editing');
        let checkbox=form.find('.access-input')

        if (isEditing) {
            form.removeClass('editing');
            form.hide();
            $('#accessText').show();
            $(this).html('<i class="bi bi-pencil icon"></i> Edit access');
            $('.access-save-button').hide();
            if (checkbox.prop('checked') != originalCheckboxState) {
                checkbox.prop('checked', originalCheckboxState); // Set to true for checked or false for unchecked
            }
        } else {
            form.find('.access-input')
            form.addClass('editing');
            form.show();
            $('#accessText').hide();
            $(this).html('<i class="bi bi-x icon"></i> Cancel');
            $('.access-save-button').show();
            originalCheckboxState = checkbox.prop('checked');
        }
    });

    $('.access-save-button').on('click', function () {
        let form = $('#accessForm');
        let entityId = document.querySelector('.id').value;
        let isFree = form.find('.access-input').prop('checked');
        $.ajax({
            url: '/Chapters/UpdateAccess',
            type: 'POST',
            data: {
                chapterId: entityId,
                newIsFreeValue: isFree
            },
            success: function (response) {
                if (response.success) {
                    $('#accessText span').text(response.isFreeValue ? 'This chapter is free for preview' : 'This chapter is not free');
                    form.removeClass('editing');
                    form.hide();
                    $('#accessText').show();
                    $('.access-edit-button').html('<i class="bi bi-pencil icon"></i> Edit access');
                    $('.access-save-button').hide();
                    $.toast({
                        text: 'Chapter Updated',
                        position: 'top-center',
                        icon: 'success'
                    })
                } else {
                    console.error(response.error + ' Failed to save data.');
                    $.toast({
                        text: 'Failed to save data.',
                        position: 'top-center',
                        icon: 'error'
                    });
                }
            },
            error: function () {
                console.error('Error during data save request.');
            }
        });
    });

});
$('.video-edit-button').on('click', function () {
    let card = $('.video-card');
    let isEditing = card.hasClass('editing');
    if (isEditing) {
        toggleVideoEditability(false);
    } else {
        toggleVideoEditability(true);
    }

});
function toggleVideoEditability(isEditing) {
    let card = $('.video-card');
    if (isEditing) {
        card.addClass('editing');
        $('.video-edit-button').html('<i class="bi bi-x icon"></i> Cancel ');
        $('#videoUploadFormContainer').show();
        $('#video-container').hide();
    } else {
        $('.video-edit-button').html('<i class="bi bi-pencil icon"></i> Edit Video');
        $('#video-container').show();
        $('#videoUploadFormContainer').hide();
        card.removeClass('editing');
    }
}
window.toggleVideoEditability = toggleVideoEditability;
window.reloadVideoPartial = reloadVideoPartial;
window.handlePublish = handlePublish;
window.handleDelete = handleDelete;

/*

$('#addChapterBtn').on('click', function () {
    var newChapterCard = createChapterCard();
    $('#sortableChapters').append(newChapterCard);
});

function createChapterCard() {
    // Create a new chapter card with a unique ID
    var newChapterCardId = 'chapter-card-' + Math.floor(Math.random() * 1000);
    var newChapterCard = $('<div>', { class: 'chapter-card', id: newChapterCardId });

    // Add chapter title input field
    newChapterCard.append($('<input>', { type: 'text', class: 'form-control', placeholder: 'Chapter Title' }));

    // Add other chapter details as needed

    return newChapterCard;
}*/
