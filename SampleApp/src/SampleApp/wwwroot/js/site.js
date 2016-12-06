
$(".removeIcon").on('click', (function (event) {
    if (confirm("Are you sure you want to remove this item ?") == false) {
        event.preventDefault();
    }
}));