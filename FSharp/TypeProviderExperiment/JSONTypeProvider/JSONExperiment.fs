namespace JSONTypeProvider

open FSharp.Data;

type Simple = JsonProvider<""" { "name":"John", "age":94 } """>

type Numbers = JsonProvider<""" [1, 2, 3, 3.14] """>

type Mixed = JsonProvider<""" [1, 2, "hello", "world" ] """>

type People = JsonProvider<""" 
  [ { "name":"John", "age":94, "meta":3.0 }, 
    { "name":"Tomas", "meta":"foo" } ] """>

type Tweet = JsonProvider<"../JSONTypeProvider/data/TwitterStream.json", SampleIsList=true, EmbeddedResource="MyLib, TwitterStream.json">

type JSONExperiment () =

    member this.TestThis () =
        //Simple
        let simple = Simple.Parse(""" { "name":"Tomas", "age":4 } """)
        let a = simple.Age
        let b = simple.Name

        //Numbers
        let nums = Numbers.Parse(""" [1.2, 45.1, 98.2, 5] """)
        let total = nums |> Seq.sum

        //Mixed
        let mixed = Mixed.Parse(""" [4, 5, "hello", "world", "3" ] """)
        let a1 = mixed.Numbers |> Seq.sum
        let b1 = mixed.Strings |> String.concat ", "
        
        //Records
        for item in People.GetSamples() do 
          printf "%s " item.Name
          item.Age |> Option.iter (printf "(%d)")
          printfn ""

        //twitter
        let text = """ 
          {"in_reply_to_status_id_str":null,"text":"Wonder If Creshawna Smoking .","in_reply_to_user_id_str":null,"retweet_count":0,"geo":null,"source":"\u003Ca href=\"http:\/\/twitter.com\/download\/android\" rel=\"nofollow\"\u003ETwitter for Android\u003C\/a\u003E","retweeted":false,"truncated":false,"id_str":"263290764660969473","entities":{"user_mentions":[],"hashtags":[],"urls":[]},"in_reply_to_user_id":null,"in_reply_to_status_id":null,"place":null,"coordinates":null,"in_reply_to_screen_name":null,"created_at":"Tue Oct 30 14:46:24 +0000 2012","user":{"notifications":null,"contributors_enabled":false,"time_zone":null,"profile_background_color":"","location":"With My Love KENNY .","profile_background_tile":true,"profile_image_url_https":"https:\/\/si0.twimg.com\/profile_images\/2764601408\/89b2c2799396d5c2190f48a4f47d9e28_normal.jpeg","default_profile_image":false,"follow_request_sent":null,"profile_sidebar_fill_color":"DDEEF6","description":"Fuck Ah BIO ' Just Follow ME . ","profile_banner_url":"https:\/\/si0.twimg.com\/profile_banners\/739055311\/1349564626","favourites_count":91,"screen_name":"2TiGhT_4YoU","profile_sidebar_border_color":"fff","id_str":"739055311","verified":false,"lang":"en","statuses_count":10363,"profile_use_background_image":true,"protected":false,"profile_image_url":"http:\/\/a0.twimg.com\/profile_images\/2764601408\/89b2c2799396d5c2190f48a4f47d9e28_normal.jpeg","listed_count":0,"geo_enabled":false,"created_at":"Sun Aug 05 18:55:18 +0000 2012","profile_text_color":"333333","name":"BabyDOLL : )","profile_background_image_url":"http:\/\/a0.twimg.com\/profile_background_images\/692115804\/971d7409058ddbbecf73cee376f9bb75.jpeg","friends_count":313,"url":null,"id":739055311,"is_translator":false,"default_profile":false,"following":null,"profile_background_image_url_https":"https:\/\/si0.twimg.com\/profile_background_images\/692115804\/971d7409058ddbbecf73cee376f9bb75.jpeg","utc_offset":null,"profile_link_color":"51B342","followers_count":372},"id":263290764660969473,"contributors":null,"favorited":false}
            """
        let tweet = Tweet.Parse(text)

        printfn "%s (retweeted %d times)\n:%s"
          tweet.User.Value.Name tweet.RetweetCount.Value tweet.Text.Value

        0

    member this.Tweet text = Tweet.Parse text

