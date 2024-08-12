export function scrollToMessagesBottom() {
    var element = document.querySelector('.chat-container');
    element.scrollTop = element.scrollHeight;
}

export function typeText(elementId, text) {
    var timerId = setInterval(function () {
        var element = document.getElementById(elementId);
        if (element) {
            element.textContent += text[0];
            text = text.substring(1);
            if (text.length === 0) {
                clearInterval(timerId);
            }
        }
        else {
            console.error("Element not found with ID:", elementId);
            clearInterval(timerId);
        }

        scrollToMessagesBottom();
    }, 10);
}
