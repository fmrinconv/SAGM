showInPopup = (url, title, classname) => {
    $.ajax({
        type: 'GET',
        url: url,
        success: function (res) {
            $('#form-modal .modal-header').removeClass("bg-primary");
            $('#form-modal .modal-header').removeClass("text-white");
            $('#form-modal .modal-header').removeClass("bg-warning");
            $('#form-modal .modal-header').removeClass("text-black");
            
            $('#form-modal .modal-header').addClass(classname);
            if (classname == "bg-primary") {
                $('#form-modal .modal-header').addClass("text-white");
            }
            else {
                $('#form-modal .modal-header').addClass("text-black");
            }
            $('#form-modal .modal-body').html(res);
            $('#form-modal .modal-title').html(title);
            $('#form-modal').modal('show');
            setTimeout(function () {
                $("#form-modal input:text, #form-modal textarea").first().focus();
            }, 600);

        }
    })
}

jQueryAjaxPost = form => {
    try {
        $.ajax({
            type: 'POST',
            url: form.action,
            data: new FormData(form),
            contentType: false,
            processData: false,
            success: function (res) {
                if (res.isValid) {
                    $('#view-all').html(res.html)
                    $('#form-modal .modal-body').html('');
                    $('#form-modal .modal-title').html('');
                    $('#form-modal').modal('hide');
                    // reload the table         
                    location.reload()

                }
                else

                    $('#form-modal .modal-body').html(res.html);
            },
            error: function (err) {
                console.log(err)
            }
        })
        //to prevent default form submit event
        return false;
    } catch (ex) {
        console.log(ex)
    }
}

(function (soccerDeleteDialog) {

    var methods = {
        "openModal": openModal,
        "deleteItem": deleteItem
    };

    var item_to_delete;

    /**
         * Open a modal by class name or Id.
         *
         * @return string id item.
         */
    function openModal(modalName, classOrId, sourceEvent, deletePath, eventClassOrId) {
        var textEvent;
        if (classOrId) {
            textEvent = "." + modalName;
        } else {
            textEvent = "#" + modalName;
        }

        $(textEvent).click((e) => {
            item_to_delete = e.currentTarget.dataset.id;
            deleteItem(sourceEvent, deletePath, eventClassOrId);
        });
    }

    /**
     * Path to delete an item.
     *
     * @return void.
     */
    function deleteItem(sourceEvent, deletePath, eventClassOrId) {
        var textEvent;
        if (eventClassOrId) {
            textEvent = "." + sourceEvent;
        } else {
            textEvent = "#" + sourceEvent;
        }
        $(textEvent).click(function () {
            window.location.href = deletePath + item_to_delete;
        });
    }

    soccerDeleteDialog.sc_deleteDialog = methods;

})(window);