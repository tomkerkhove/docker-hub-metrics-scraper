# Docker Hub Metrics Scraper
A scraper for Docker Hub metrics making the data available for analysis by reporting the following metrics in Azure Application Insights:

- `Image Pulls` - Amount of pulls for the image, regardless of the tag
- `Image Stars` - Amount of stars for the image

![Result in Application Insights](./media/result.png)

## How it works

An Azure Function will scrape Docker Hub API for every scrape request on a Service Bus queue and report the following metrics in Azure Application Insights.

![How it works](./media/how-it-works.png)

We provide an Azure Logic App which you can deploy per repo and image to schedule scrapes and report them as multi-dimensional metrics.

## Deploying a scrape trigger

We provide an Azure Logic App which you can deploy per repo and image to schedule scrapes and report them as multi-dimensional metrics.

<a href="https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2Ftomkerkhove%2Fdocker-hub-metrics-scraper%2Fmaster%2Fdeploy%2Fscrape-trigger.json" target="_blank">
    <img src="https://azuredeploy.net/deploybutton.png"/>
</a>

## License

This is licensed under The MIT License (MIT). Which means that you can use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the web application. But you always need to state that Tom Kerkhove is the original author of this web application.
