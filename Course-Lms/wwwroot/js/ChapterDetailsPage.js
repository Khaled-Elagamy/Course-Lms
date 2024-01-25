import 'https://cdnjs.cloudflare.com/ajax/libs/jquery-toast-plugin/1.3.2/jquery.toast.min.js';

$(document).on("click", ".chapterbtn", navigateToChapter);

$(document).ready(function () {

    window.onpopstate = function () {
        $.get(document.location, function (data) {
            $("#ChapterData").html(data);
        });
    };


});
let lastChapterId = null;
function navigateToChapter() {
    let chapterId = $(this).data("chapter-id");
    if (chapterId !== lastChapterId) {

        let url = document.URL
        let courseNameStartIndex = url.indexOf('/Courses/') + '/Courses/'.length;
        let courseNameEndIndex = url.indexOf('/', courseNameStartIndex);
        let courseName = url.substring(courseNameStartIndex, courseNameEndIndex);
        let requesturl = `/Courses/${courseName}/Chapters/${chapterId}`;

        history.pushState(null, null, requesturl);

        $.ajax({
            url: requesturl,
            type: "GET",
            success: function (data) {
                $("#ChapterData").html(data);
            },
            error: function (xhr) {
                $.toast({
                    heading: 'ServerError',
                    text: `Error during navigating\n${xhr.responseText}`,
                    position: 'top-center',
                    icon: 'error'
                })
            }
        });   // Update the last clicked chapter
        lastChapterId = chapterId;
    }
};
function completeChapter(chapterId, nextChapterId, courseName) {
    console.log("name" + courseName);
    let dataObject = {
        chapterId: chapterId,
        ...(nextChapterId > 0 ? { nextChapterId: nextChapterId } : {}),
        courseName: courseName
    };
    console.log("data" + dataObject);
    $.ajax({
        url: '/Chapters/CompleteChapter',
        type: 'POST',
        data: dataObject,
        success: function (data) {

            if (data.success) {
                $('#ProgressButton').html(data.button);
                $.toast({
                    text: data.message,
                    position: 'top-center',
                    icon: 'success'
                })
                $('#completedNotification').fadeIn();
                let localStorageKey = "confetti_loaded_" + courseName;
                let firstTime = localStorage.getItem(localStorageKey);

                if (firstTime !== 'yes') {
                    localStorage.setItem(localStorageKey, "yes");
                    startConfetti();
                }

            } else {
                // Update the ChapterData div with the new chapter content
                $('#ChapterData').html(data.nextChapterView);
                $.toast({
                    text: 'Progress Updated',
                    position: 'top-center',
                    icon: 'success'
                })
            }
            $('#SideBarItems').html(data.sideBarItems);
            $('#ProgressBar').html(data.progressBar);
            $(".chapterbtn[data-chapter-id=" + chapterId + "]").addClass("isActive");

        },
        error: function (error) {
            console.error('Error:', error);
            $.toast({
                text: 'Failed to save progress.',
                position: 'top-center',
                icon: 'error'
            })
        }
    });
    // Handle back/forward buttons
}
function unCompleteChapter(chapterId) {

    $.ajax({
        url: '/Chapters/UnCompleteChapter',
        type: 'POST',
        data: { chapterId: chapterId },
        success: function (data) {
            if (data.success) {
                $('#ProgressButton').html(data.button);
                $('#completedNotification').fadeOut();
                $.toast({
                    text: 'Progress Updated',
                    position: 'top-center',
                    icon: 'success'
                })
                $('#SideBarItems').html(data.sideBarItems);
                $('#ProgressBar').html(data.progressBar);
                $(".chapterbtn[data-chapter-id=" + chapterId + "]").addClass("isActive");

            } else {
                $.toast({
                    text: 'Failed to save progress.',
                    position: 'top-center',
                    icon: 'error'
                })
            }
        },
        error: function (error) {
            console.error('Error:', error);
            $.toast({
                text: 'Failed to save progress.',
                position: 'top-center',
                icon: 'error'
            })
        }
    });
}

function startConfetti() {
    // Configure confetti settings
    const defaults = {
        origin: { y: 0 },
        max: 150,
        size: 1,
        rotate: true,
        gravity: 0.1,
        spread: 360,
        ticks: 100,
        start_from_edge: true,
        colors: ['#FFD700', '#FF4500', '#00CED1', '#FF69B4', '#1E90FF'],
    };


    confetti({
        ...defaults,
        particleCount: 400,
        scalar: 2,
    });
    // Stop confetti after 10 seconds

}


window.completeChapter = completeChapter;
window.unCompleteChapter = unCompleteChapter;
