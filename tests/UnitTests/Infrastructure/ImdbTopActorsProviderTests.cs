using System.Net;
using FluentAssertions;
using SplititAssignment.Infrastructure.Providers;

namespace SplititAssignment.UnitTests.Infrastructure;

public class ImdbTopActorsProviderTests
{
    private sealed class FakeHandler : HttpMessageHandler
    {
        private readonly string _html;
        public FakeHandler(string html) => _html = html;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(_html)
            });
    }

    [Fact]
    public async Task FetchAsync_ParsesActors_FromSampleHtml()
    {
        const string sampleHtml = @"
<html><body>
<div class='lister-item'>
  <span class='lister-item-index'>1.</span>
  <h3><a href='/name/nm0000158/'>Tom Hanks</a></h3>
  <img src='http://img/tom.jpg' />
  <p class='text-small'>Known for Forrest Gump</p>
</div>
<div class='lister-item'>
  <span class='lister-item-index'>2.</span>
  <h3><a href='/name/nm0000138/'>Leonardo DiCaprio</a></h3>
  <img src='http://img/leo.jpg' />
  <p class='text-small'>Known for Inception</p>
</div>
</body></html>";

        var http = new HttpClient(new FakeHandler(sampleHtml));
        var provider = new ImdbTopActorsProvider(http);

        var list = await provider.FetchAsync();

        list.Should().HaveCount(2);
        list[0].Name.Should().Be("Tom Hanks");
        list[0].Rank.Should().Be(1);

        list[1].Name.Should().Be("Leonardo DiCaprio");
        list[1].Rank.Should().Be(2);
    }
}
