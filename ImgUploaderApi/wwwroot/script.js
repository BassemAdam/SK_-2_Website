  $(document).ready(function () {
            $('#imageUploadForm').submit(function (event) {
                event.preventDefault();

                var formData = new FormData();
                formData.append('title', $('#imageTitle').val());
                formData.append('image', $('#imageFile')[0].files[0]);

                $.ajax({
                    url: '/upload',
                    type: 'POST',
                    data: formData,
                    processData: false,
                    contentType: false,
                    success: function (data) {
                        // Redirect to the page showing the uploaded image
                        window.location.href = '/picture/' + data.id;
                    },
                    error: function (xhr, status, error) {
                        console.error(xhr);
                        alert('Error uploading image.');
                    }
                });
            });
        });
//as alternative to ajax i could have used fetch api instead of jquery ajax
        // fetch('/upload', {
        //     method: 'POST',
        //     body: formData
        // }).then(response => {
        //     if (!response.ok) {
        //         throw new Error('Network response was not ok');
        //     }
        //     return response.json();
        // }).then(data => {
        //     window.location.href = '/picture/' + data.id;
        // }).catch(error => {
        //     console.error('There has been a problem with your fetch operation:', error);
        // });


// Fetch is more powerful for those reasons: however it is not supported in older browsers
// Built-in Promises: Fetch uses Promises by default, which makes it easier to handle asynchronous operations and avoid callback hell. Promises also allow you to use async/await syntax for even cleaner code.

// Request and Response Objects: Fetch introduces Request and Response objects, which provide a more powerful and flexible way to configure requests and handle responses.

// Streaming: Fetch supports streaming responses, which allows you to start processing parts of the response while the rest is still downloading. This can improve performance for large responses.

// Service Worker Integration: Fetch is designed to work well with Service Workers, which are a key part of Progressive Web Apps (PWAs). Service Workers can intercept Fetch requests and provide custom responses, which can be used to implement offline functionality, caching, and more.

// Standardized and Future-Proof: Fetch is a web standard, which means it's implemented consistently across browsers and is likely to be supported for a long time. jQuery's $.ajax function, on the other hand, is not a standard and its behavior can vary between different versions of jQuery.