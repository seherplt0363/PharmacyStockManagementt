"use strict";

/* ============================================================
   PHARMACY STOCK MANAGEMENT
   DASHBOARD CHARTS
   ============================================================ */

const data = window.dashboardData || {};


/* ============================================================
   THEME COLORS
   ============================================================ */

function getThemeColors() {

    const isDark = document.body.classList.contains("dark-mode");

    return {

        textColor: isDark
            ? "#cbd5e1"
            : "#64748b",

        titleColor: isDark
            ? "#f8fafc"
            : "#1e293b",

        gridColor: isDark
            ? "rgba(255,255,255,0.08)"
            : "rgba(100,116,139,0.12)",

        tooltipBackground: isDark
            ? "#1e293b"
            : "#ffffff",

        tooltipText: isDark
            ? "#f8fafc"
            : "#1e293b",

        tooltipBorder: isDark
            ? "#334155"
            : "#e2e8f0"
    };
}


let themeColors = getThemeColors();


/* ============================================================
   CHART REFERENCES
   ============================================================ */

let stockChart = null;
let topProductsChart = null;


/* ============================================================
   CHART DEFAULTS
   ============================================================ */

Chart.defaults.font.family =
    "'Inter', 'Segoe UI', Arial, sans-serif";

Chart.defaults.animation.duration = 700;

Chart.defaults.animation.easing = "easeOutQuart";


/* ============================================================
   1. SON 7 GÜNLÜK STOK HAREKETLERİ
   ============================================================ */

const stockCanvas = document.getElementById("stockChart");

if (stockCanvas) {

    stockChart = new Chart(stockCanvas, {

        type: "bar",

        data: {

            labels: data.last7Days || [],

            datasets: [

                {
                    label: "Stok Girişi",

                    data: data.stockIn || [],

                    backgroundColor: "#22b983",

                    borderColor: "#22b983",

                    borderWidth: 0,

                    borderRadius: 5,

                    borderSkipped: false,

                    barPercentage: 0.68,

                    categoryPercentage: 0.72
                },

                {
                    label: "Stok Çıkışı",

                    data: data.stockOut || [],

                    backgroundColor: "#ef4b4f",

                    borderColor: "#ef4b4f",

                    borderWidth: 0,

                    borderRadius: 5,

                    borderSkipped: false,

                    barPercentage: 0.68,

                    categoryPercentage: 0.72
                }

            ]

        },

        options: {

            responsive: true,

            maintainAspectRatio: false,

            interaction: {
                mode: "index",
                intersect: false
            },

            plugins: {

                legend: {

                    display: true,

                    position: "top",

                    align: "start",

                    labels: {

                        color: themeColors.textColor,

                        usePointStyle: true,

                        pointStyle: "circle",

                        boxWidth: 8,

                        boxHeight: 8,

                        padding: 18,

                        font: {
                            size: 12,
                            weight: "500"
                        }
                    }
                },

                tooltip: {

                    backgroundColor: themeColors.tooltipBackground,

                    titleColor: themeColors.tooltipText,

                    bodyColor: themeColors.tooltipText,

                    borderColor: themeColors.tooltipBorder,

                    borderWidth: 1,

                    padding: 12,

                    cornerRadius: 10,

                    displayColors: true,

                    titleFont: {
                        weight: "600"
                    },

                    bodyFont: {
                        size: 12
                    }
                }
            },

            scales: {

                x: {

                    stacked: false,

                    border: {
                        display: false
                    },

                    ticks: {

                        color: themeColors.textColor,

                        font: {
                            size: 10,
                            weight: "500"
                        },

                        padding: 8
                    },

                    grid: {

                        display: false
                    }
                },

                y: {

                    beginAtZero: true,

                    border: {
                        display: false
                    },

                    ticks: {

                        color: themeColors.textColor,

                        precision: 0,

                        font: {
                            size: 10
                        },

                        padding: 8
                    },

                    grid: {

                        color: themeColors.gridColor,

                        drawTicks: false
                    }
                }
            }
        }
    });
}


/* ============================================================
   2. EN ÇOK İŞLEM GÖREN ÜRÜNLER
   ============================================================ */

const topProductsCanvas =
    document.getElementById("topProductsChart");


if (topProductsCanvas) {

    topProductsChart = new Chart(topProductsCanvas, {

        type: "bar",

        data: {

            labels: data.topProductNames || [],

            datasets: [

                {

                    label: "İşlem Sayısı",

                    data: data.topProductCounts || [],

                    backgroundColor: "#5968e8",

                    hoverBackgroundColor: "#4f5bd5",

                    borderWidth: 0,

                    borderRadius: 6,

                    borderSkipped: false,

                    barPercentage: 0.58,

                    categoryPercentage: 0.72
                }

            ]

        },

        options: {

            indexAxis: "y",

            responsive: true,

            maintainAspectRatio: false,

            animation: {

                duration: 800,

                easing: "easeOutQuart"
            },

            plugins: {

                legend: {

                    display: false
                },

                tooltip: {

                    backgroundColor: themeColors.tooltipBackground,

                    titleColor: themeColors.tooltipText,

                    bodyColor: themeColors.tooltipText,

                    borderColor: themeColors.tooltipBorder,

                    borderWidth: 1,

                    padding: 12,

                    cornerRadius: 10,

                    displayColors: false,

                    callbacks: {

                        label: function (context) {

                            return " İşlem Sayısı: " +
                                context.parsed.x;
                        }
                    }
                }
            },

            scales: {

                x: {

                    beginAtZero: true,

                    border: {
                        display: false
                    },

                    ticks: {

                        color: themeColors.textColor,

                        precision: 0,

                        font: {
                            size: 10
                        },

                        padding: 6
                    },

                    grid: {

                        color: themeColors.gridColor,

                        drawTicks: false
                    }
                },

                y: {

                    border: {
                        display: false
                    },

                    ticks: {

                        color: themeColors.textColor,

                        font: {

                            size: 10,

                            weight: "500"
                        },

                        padding: 6
                    },

                    grid: {

                        display: false
                    }
                }
            }
        }
    });
}


/* ============================================================
   DARK / LIGHT MODE
   ============================================================ */

function updateChartTheme(chart) {

    if (!chart) {
        return;
    }

    const colors = getThemeColors();

    /* -------------------------
       Legend
       ------------------------- */

    if (
        chart.options.plugins &&
        chart.options.plugins.legend &&
        chart.options.plugins.legend.labels
    ) {

        chart.options.plugins.legend.labels.color =
            colors.textColor;
    }


    /* -------------------------
       Tooltip
       ------------------------- */

    if (
        chart.options.plugins &&
        chart.options.plugins.tooltip
    ) {

        chart.options.plugins.tooltip.backgroundColor =
            colors.tooltipBackground;

        chart.options.plugins.tooltip.titleColor =
            colors.tooltipText;

        chart.options.plugins.tooltip.bodyColor =
            colors.tooltipText;

        chart.options.plugins.tooltip.borderColor =
            colors.tooltipBorder;
    }


    /* -------------------------
       X Axis
       ------------------------- */

    if (
        chart.options.scales &&
        chart.options.scales.x
    ) {

        if (chart.options.scales.x.ticks) {

            chart.options.scales.x.ticks.color =
                colors.textColor;
        }

        if (chart.options.scales.x.grid) {

            chart.options.scales.x.grid.color =
                colors.gridColor;
        }
    }


    /* -------------------------
       Y Axis
       ------------------------- */

    if (
        chart.options.scales &&
        chart.options.scales.y
    ) {

        if (chart.options.scales.y.ticks) {

            chart.options.scales.y.ticks.color =
                colors.textColor;
        }

        if (chart.options.scales.y.grid) {

            chart.options.scales.y.grid.color =
                colors.gridColor;
        }
    }


    chart.update();
}


/* ============================================================
   THEME TOGGLE
   ============================================================ */

const themeToggleButton =
    document.getElementById("themeToggle");


if (themeToggleButton) {

    themeToggleButton.addEventListener(
        "click",
        function () {

            setTimeout(function () {

                themeColors = getThemeColors();

                updateChartTheme(stockChart);

                updateChartTheme(topProductsChart);

            }, 150);

        }
    );
}


/* ============================================================
   CARD ANIMATION
   ============================================================ */

document.addEventListener(
    "DOMContentLoaded",
    function () {

        const chartCards =
            document.querySelectorAll(
                ".dashboard-chart-card"
            );


        chartCards.forEach(
            function (card, index) {

                card.style.opacity = "0";

                card.style.transform =
                    "translateY(15px)";


                setTimeout(
                    function () {

                        card.style.transition =
                            "opacity .45s ease, transform .45s ease";

                        card.style.opacity = "1";

                        card.style.transform =
                            "translateY(0)";

                    },
                    index * 120
                );
            }
        );

    }
);