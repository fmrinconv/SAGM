showInPopup = (url, title, classname) => {
    $.ajax({
        type: 'GET',
        url: url,
        success: function (res) {
            $('#form-modal .modal-header').removeClass("bg-primary");
            $('#form-modal .modal-header').removeClass("text-white");
            $('#form-modal .modal-header').removeClass("bg-warning");
            $('#form-modal .modal-header').removeClass("text-black");
            $('#form-modal .modal-header').removeClass("bg-danger");
            
            $('#form-modal .modal-header').addClass(classname);
            if (classname == "bg-primary" ) {
                $('#form-modal .modal-header').addClass("text-white");
            }   
            else if (classname == "bg-warning") {   
                $('#form-modal .modal-header').addClass("text-black");
            }
            else if (classname == "bg-secondary") {
                $('#form-modal .modal-header').removeClass("bg-secondary");
                $('#form-modal .modal-header').addClass("text-danger");
                $('#form-modal .modal-header').css("background-color", "gainsboro")
            }
            else {
                $('#form-modal .modal-header').addClass("text-white");
            }

            if (title.substring(0, 16) == "Crear cotización" || title.substring(0, 17) == "Editar cotización" || title.substring(0, 15) == "Agregar partida") {
                $('#form-modal .modal-icon').addClass("fa fa-file-invoice-dollar");
            }
            else if (title == "Hacer comentario")
            {
                $('#form-modal .modal-icon').addClass("fa fa-comments");
            }
            else if (title == "Agregar archivos") {
                $('#form-modal .modal-icon').addClass("fa fa-file-alt");
            }
            else if (title == "Crear Orden de trabajo") {
                $('#form-modal .modal-icon').addClass("fa fa-share-from-square");
            }
            else if (title == "Editar Orden de trabajo") {
                $('#form-modal .modal-icon').addClass("fa fa-wrench");
            }
            else if (title == "Eliminar archivo") {
                $('#form-modal .modal-icon').addClass("fa fa-triangle-exclamation");
            }
            else if (title == "Agregar proceso") {
                $('#form-modal .modal-icon').addClass("fa-solid fa-gear");
            }
            else if (title == "Eliminar proceso") {
                $('#form-modal .modal-icon').addClass("fa-solid fa-gear");
            }
            else {
                $('#form-modal .modal-icon').addClass("fa fa-globe");
            }
            $('#form-modal .modal-body').html(res);
            $('#form-modal .modal-title').html(title);
            $('#form-modal').modal('show');
            setTimeout(function () {
                $("#form-modal select, #form-modal input:text, #form-modal textarea, #form-control textarea").first().focus();
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