document.addEventListener('DOMContentLoaded', function () {
    const contentTextarea = document.querySelector('textarea.article-content');

    if (contentTextarea) {
        tinymce.init({
            selector: 'textarea.article-content',
            encoding: 'xml',
            entity_encoding: 'raw',
            promotion: false,
            statusbar: false,
            height: 500,
            plugins: [
                'advlist', 'autolink', 'lists', 'link', 'charmap', 'anchor', 'searchreplace', 'visualblocks', 'code',
                'insertdatetime', 'table', 'wordcount'
            ],
            toolbar: 'undo redo | blocks | bold italic forecolor backcolor | ' +
                'alignleft aligncenter alignright alignjustify | ' +
                'bullist numlist outdent indent | link | ' +
                'removeformat | code',
            menubar: 'format tools table',
            branding: false,
            resize: 'vertical',
            paste_data_images: true,
            automatic_uploads: false,
            setup: function (editor) {
                editor.on('change', function () {
                    editor.save();
                });
            }
        });

        const form = document.getElementById('articleForm');
        if (form) {
            form.addEventListener('submit', function () {
                tinymce.triggerSave();
            });
        }
    }
});