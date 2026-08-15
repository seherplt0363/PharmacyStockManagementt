document.addEventListener("DOMContentLoaded", function () {

    // =====================================================
    // ELEMENTLER
    // =====================================================

    const searchInput =
        document.getElementById("orderSearchInput");

    const filterButtons =
        document.querySelectorAll(".filter-btn");

    const rows =
        document.querySelectorAll(".order-row");


    // =====================================================
    // AKTİF FİLTRE
    // =====================================================

    let currentFilter = "all";


    // =====================================================
    // TABLOYU FİLTRELE
    // =====================================================

    function filterTable() {

        // Arama kutusu yoksa boş değer kullan
        const searchTerm =
            searchInput
                ? searchInput.value
                    .toLocaleLowerCase("tr-TR")
                    .trim()
                : "";


        rows.forEach(function (row) {

            // Satırdaki bütün yazıları al
            const rowText =
                row.textContent
                    .toLocaleLowerCase("tr-TR");


            // Razor tarafından oluşturulan data-status
            const status =
                row.getAttribute("data-status");


            // Arama kontrolü
            const matchesSearch =
                rowText.includes(searchTerm);


            // Durum filtresi kontrolü
            const matchesFilter =
                currentFilter === "all"
                || status === currentFilter;


            // Her iki koşul sağlanıyorsa satırı göster
            row.style.display =
                matchesSearch && matchesFilter
                    ? ""
                    : "none";

        });

    }


    // =====================================================
    // ARAMA
    // =====================================================

    if (searchInput) {

        searchInput.addEventListener(
            "input",
            filterTable
        );

    }


    // =====================================================
    // DURUM FİLTRELERİ
    // =====================================================

    filterButtons.forEach(function (button) {

        button.addEventListener("click", function () {

            // Önce bütün butonlardan active kaldır
            filterButtons.forEach(function (item) {

                item.classList.remove("active");

            });


            // Tıklanan butonu aktif yap
            this.classList.add("active");


            // Seçilen filtreyi al
            currentFilter =
                this.getAttribute("data-filter");


            // Tabloyu tekrar filtrele
            filterTable();

        });

    });

});