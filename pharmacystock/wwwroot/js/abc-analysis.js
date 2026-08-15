document.addEventListener("DOMContentLoaded", function () {

    // =====================================================
    // ELEMENTLER
    // =====================================================

    const page =
        document.getElementById("abcAnalysisPage");

    const tableSearch =
        document.getElementById("tableSearch");

    const table =
        document.getElementById("abcTable");

    const excelButton =
        document.getElementById("btnExportExcel");

    const pdfButton =
        document.getElementById("btnExportPdf");

    const printButton =
        document.getElementById("btnPrint");


    if (!page) {
        return;
    }


    // =====================================================
    // RAZOR VERİLERİNİ HTML DATA ATTRIBUTE'LARINDAN AL
    // =====================================================

    const totalProducts =
        Number(page.dataset.totalProducts || 0);

    const groupACount =
        Number(page.dataset.groupA || 0);

    const groupBCount =
        Number(page.dataset.groupB || 0);

    const groupCCount =
        Number(page.dataset.groupC || 0);


    let topProductNames = [];
    let topProductValues = [];


    try {

        topProductNames =
            JSON.parse(
                page.dataset.topProductNames || "[]"
            );

        topProductValues =
            JSON.parse(
                page.dataset.topProductValues || "[]"
            );

    }
    catch (error) {

        console.error(
            "ABC grafik verileri okunamadı:",
            error
        );

    }


    // =====================================================
    // TEMA RENKLERİ
    // =====================================================

    function isDarkMode() {

        return document.body.classList.contains(
            "dark-mode"
        );

    }


    function getThemeColors() {

        return {

            textPrimary:
                isDarkMode()
                    ? "#f8fafc"
                    : "#212529",

            textSecondary:
                isDarkMode()
                    ? "#94a3b8"
                    : "#6c757d",

            grid:
                isDarkMode()
                    ? "rgba(255, 255, 255, 0.05)"
                    : "rgba(0, 0, 0, 0.06)",

            border:
                isDarkMode()
                    ? "#2b3448"
                    : "#ffffff"

        };

    }


    // =====================================================
    // DOUGHNUT ORTA YAZI PLUGIN
    // =====================================================

    const doughnutCenterPlugin = {

        id: "doughnutCenterText",

        afterDraw: function (chart) {

            if (chart.config.type !== "doughnut") {
                return;
            }


            const meta =
                chart.getDatasetMeta(0);


            if (!meta.data || meta.data.length === 0) {
                return;
            }


            const ctx =
                chart.ctx;

            const x =
                meta.data[0].x;

            const y =
                meta.data[0].y;


            const colors =
                getThemeColors();


            ctx.save();

            ctx.textAlign =
                "center";

            ctx.textBaseline =
                "middle";


            // Toplam ürün sayısı

            ctx.fillStyle =
                colors.textPrimary;

            ctx.font =
                "bold 20px sans-serif";

            ctx.fillText(
                totalProducts,
                x,
                y - 10
            );


            // Alt açıklama

            ctx.fillStyle =
                colors.textSecondary;

            ctx.font =
                "12px sans-serif";

            ctx.fillText(
                "Toplam Ürün",
                x,
                y + 12
            );


            ctx.restore();

        }

    };


    // =====================================================
    // DOUGHNUT CHART
    // =====================================================

    const doughnutCanvas =
        document.getElementById(
            "abcDoughnutChart"
        );


    let doughnutChart = null;


    if (doughnutCanvas) {

        const colors =
            getThemeColors();


        doughnutChart =
            new Chart(
                doughnutCanvas.getContext("2d"),
                {

                    type: "doughnut",

                    data: {

                        labels: [
                            "A Grubu",
                            "B Grubu",
                            "C Grubu"
                        ],

                        datasets: [

                            {

                                data: [
                                    groupACount,
                                    groupBCount,
                                    groupCCount
                                ],

                                backgroundColor: [
                                    "#22c55e",
                                    "#f59e0b",
                                    "#ef4444"
                                ],

                                borderColor:
                                    colors.border,

                                borderWidth: 2,

                                hoverOffset: 6

                            }

                        ]

                    },

                    options: {

                        responsive: true,

                        maintainAspectRatio: false,

                        cutout: "70%",

                        plugins: {

                            legend: {

                                position: "right",

                                labels: {

                                    color:
                                        colors.textSecondary,

                                    boxWidth: 12,

                                    padding: 15,

                                    font: {
                                        size: 12,
                                        weight: "600"
                                    }

                                }

                            },

                            tooltip: {

                                backgroundColor:
                                    "#111827",

                                cornerRadius: 8

                            }

                        }

                    },

                    plugins: [
                        doughnutCenterPlugin
                    ]

                }
            );

    }


    // =====================================================
    // BAR CHART
    // =====================================================

    const barCanvas =
        document.getElementById(
            "topProductsBarChart"
        );


    let barChart = null;


    if (barCanvas) {

        const colors =
            getThemeColors();


        barChart =
            new Chart(
                barCanvas.getContext("2d"),
                {

                    type: "bar",

                    data: {

                        labels:
                            topProductNames,

                        datasets: [

                            {

                                label:
                                    "Yıllık Değer (₺)",

                                data:
                                    topProductValues,

                                backgroundColor:
                                    "#3b82f6",

                                borderRadius:
                                    6,

                                maxBarThickness:
                                    35

                            }

                        ]

                    },

                    options: {

                        responsive: true,

                        maintainAspectRatio: false,

                        plugins: {

                            legend: {
                                display: false
                            },

                            tooltip: {

                                backgroundColor:
                                    "#111827",

                                cornerRadius:
                                    8,

                                callbacks: {

                                    label: function (context) {

                                        return (
                                            " ₺"
                                            + Number(
                                                context.raw
                                            )
                                                .toLocaleString(
                                                    "tr-TR",
                                                    {
                                                        minimumFractionDigits: 2
                                                    }
                                                )
                                        );

                                    }

                                }

                            }

                        },

                        scales: {

                            x: {

                                grid: {
                                    display: false
                                },

                                ticks: {

                                    color:
                                        colors.textSecondary,

                                    font: {
                                        weight: "600"
                                    }

                                }

                            },

                            y: {

                                beginAtZero:
                                    true,

                                grid: {

                                    color:
                                        colors.grid

                                },

                                ticks: {

                                    color:
                                        colors.textSecondary,

                                    callback:
                                        function (value) {

                                            return (
                                                "₺"
                                                + Number(value)
                                                    .toLocaleString(
                                                        "tr-TR"
                                                    )
                                            );

                                        }

                                }

                            }

                        }

                    }

                }
            );

    }


    // =====================================================
    // TABLO ARAMA
    // =====================================================

    if (tableSearch && table) {

        tableSearch.addEventListener(
            "input",
            function () {

                const filter =
                    this.value
                        .toLocaleLowerCase("tr-TR")
                        .trim();


                const rows =
                    table.querySelectorAll(
                        "tbody tr"
                    );


                rows.forEach(
                    function (row) {

                        const text =
                            row.textContent
                                .toLocaleLowerCase(
                                    "tr-TR"
                                );


                        row.style.display =
                            text.includes(filter)
                                ? ""
                                : "none";

                    }
                );

            }
        );

    }


    // =====================================================
    // EXCEL
    // =====================================================

    if (excelButton) {

        excelButton.addEventListener(
            "click",
            function () {

                if (
                    typeof toastr !== "undefined"
                ) {

                    toastr.info(
                        "Excel çıktısı hazırlanıyor..."
                    );

                }

            }
        );

    }


    // =====================================================
    // PDF
    // =====================================================

    if (pdfButton) {

        pdfButton.addEventListener(
            "click",
            function () {

                if (
                    typeof toastr !== "undefined"
                ) {

                    toastr.info(
                        "PDF dosyası oluşturuluyor..."
                    );

                }

            }
        );

    }


    // =====================================================
    // YAZDIR
    // =====================================================

    if (printButton) {

        printButton.addEventListener(
            "click",
            function () {

                window.print();

            }
        );

    }

});