document.addEventListener("DOMContentLoaded", function () {

    // =====================================================
    // ELEMENTLER
    // =====================================================

    const orderDraftPage =
        document.getElementById("orderDraftPage");

    const tableSearch =
        document.getElementById("tableSearch");

    const draftTableBody =
        document.getElementById("draftTableBody");

    const printButton =
        document.getElementById("btnPrint");

    const sendButton =
        document.getElementById("btnConfirmSend");

    const supplierSelect =
        document.getElementById("supplierSelect");

    const spinner =
        document.getElementById("btnSendSpinner");

    const sendText =
        document.getElementById("btnSendText");


    // =====================================================
    // TABLODA ARAMA
    // =====================================================

    if (tableSearch && draftTableBody) {

        tableSearch.addEventListener("input", function () {

            const searchValue =
                this.value
                    .toLocaleLowerCase("tr-TR")
                    .trim();


            const rows =
                draftTableBody.querySelectorAll("tr");


            rows.forEach(function (row) {

                const rowText =
                    row.textContent
                        .toLocaleLowerCase("tr-TR");


                row.style.display =
                    rowText.includes(searchValue)
                        ? ""
                        : "none";

            });

        });

    }


    // =====================================================
    // YAZDIR
    // =====================================================

    if (printButton) {

        printButton.addEventListener("click", function () {

            window.print();

        });

    }


    // =====================================================
    // TEDARİKÇİYE GÖNDER
    // =====================================================

    if (sendButton && supplierSelect && orderDraftPage) {

        sendButton.addEventListener("click", function () {

            const supplierId =
                supplierSelect.value;


            // ---------------------------------------------
            // Tedarikçi kontrolü
            // ---------------------------------------------

            if (!supplierId) {

                toastr.warning(
                    "Lütfen bir tedarikçi seçiniz."
                );

                return;

            }


            // ---------------------------------------------
            // Endpoint
            // ---------------------------------------------

            const sendUrl =
                orderDraftPage.dataset.sendUrl;


            if (!sendUrl) {

                toastr.error(
                    "Gönderim adresi bulunamadı."
                );

                return;

            }


            // ---------------------------------------------
            // Anti Forgery Token
            // ---------------------------------------------

            const tokenInput =
                document.querySelector(
                    'input[name="__RequestVerificationToken"]'
                );


            if (!tokenInput) {

                toastr.error(
                    "Güvenlik doğrulama anahtarı bulunamadı."
                );

                return;

            }


            // ---------------------------------------------
            // Butonu loading durumuna getir
            // ---------------------------------------------

            setLoadingState(true);


            // ---------------------------------------------
            // AJAX POST
            // ---------------------------------------------

            $.ajax({

                url: sendUrl,

                type: "POST",

                data: {

                    supplierId: supplierId,

                    __RequestVerificationToken:
                        tokenInput.value

                },


                // =========================================
                // BAŞARILI HTTP RESPONSE
                // =========================================

                success: function (response) {

                    setLoadingState(false);


                    if (response && response.success) {

                        toastr.success(
                            response.message
                        );


                        closeSupplierModal();

                    }
                    else {

                        toastr.error(
                            response?.message
                            ?? "Sipariş gönderilemedi."
                        );

                    }

                },


                // =========================================
                // HTTP / SERVER HATASI
                // =========================================

                error: function () {

                    setLoadingState(false);


                    toastr.error(
                        "Sunucu ile iletişim kurulurken bir hata oluştu."
                    );

                }

            });

        });

    }


    // =====================================================
    // LOADING STATE
    // =====================================================

    function setLoadingState(isLoading) {

        if (!sendButton) {
            return;
        }


        sendButton.disabled =
            isLoading;


        if (spinner) {

            spinner.classList.toggle(
                "d-none",
                !isLoading
            );

        }


        if (sendText) {

            sendText.innerText =
                isLoading
                    ? "Gönderiliyor..."
                    : "Onayla ve Gönder";

        }

    }


    // =====================================================
    // MODALI KAPAT
    // =====================================================

    function closeSupplierModal() {

        const modalElement =
            document.getElementById(
                "sendSupplierModal"
            );


        if (!modalElement) {
            return;
        }


        const modal =
            bootstrap.Modal.getInstance(
                modalElement
            )
            || new bootstrap.Modal(
                modalElement
            );


        modal.hide();

    }

});