// Attendance Heatmap using cal-heatmap
function initAttendanceHeatmap(data, isManager) {
    const cal = new CalHeatmap();
    
    // Transform data to cal-heatmap format
    const heatmapData = {};
    data.forEach(item => {
        const timestamp = new Date(item.date).getTime() / 1000;
        heatmapData[timestamp] = item.count;
    });

    const now = new Date();
    const oneYearAgo = new Date();
    oneYearAgo.setFullYear(now.getFullYear() - 1);

    cal.paint({
        itemSelector: '#attendance-heatmap',
        data: {
            source: heatmapData,
            type: 'json',
            x: d => +d,
            y: d => +d,
        },
        date: {
            start: oneYearAgo,
            max: now,
        },
        range: 12, // Show 12 months
        scale: {
            color: {
                type: 'threshold',
                range: ['#ebedf0', '#9be9a8', '#40c463', '#30a14e', '#216e39'],
                domain: [1, 3, 5, 7],
            },
        },
        domain: {
            type: 'month',
            gutter: 4,
            label: { text: 'MMM', textAlign: 'start', position: 'top' },
        },
        subDomain: {
            type: 'day',
            radius: 2,
            width: 11,
            height: 11,
            gutter: 4,
            label: 'D',
        },
        itemSelector: '#attendance-heatmap',
        theme: 'light',
    }, [
        [
            Tooltip,
            {
                text: function (date, value, dayjsDate) {
                    const dateStr = dayjsDate.format('MMM DD, YYYY');
                    const countText = value ? `${value} attendance${value > 1 ? 's' : ''}` : 'No attendance';
                    return `${dateStr}: ${countText}`;
                },
            },
        ],
        [
            LegendLite,
            {
                itemSelector: '#attendance-heatmap-legend',
                radius: 2,
                width: 11,
                height: 11,
                gutter: 4,
            },
        ],
    ]);
}
