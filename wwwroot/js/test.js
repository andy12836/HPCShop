$(document).ready(function () {
    // Event listener for the button click
    $('#createDiv').click(function () {
        // Create a new blank div element with jQuery
        var newDiv = $('<div>', {
            id: 'newDivSection', // Optional: ID for the new div
            css: {
                'background-color': '#f0f0f0',
                'width': '200px',
                'height': '100px',
                'margin': '10px',
                'border': '1px solid #000'
            }
        });

        // Append the newly created blank div to the body or a specific section
        $('body').append(newDiv);
    });
});