using System.Threading;

namespace okta_aws_cli.net.Authentication
{
    public sealed class BrowserAuthentication
    {
        // Trade-off: JavaFX app model makes interacting with UI state challenging
        // Experienced JavaFX devs welcomed to suggest solutions to this
        private static readonly CountdownEvent USER_AUTH_COMPLETE = new CountdownEvent(1);

        // Trade-off: JavaFX app model makes passing parameters to UI challenging
        // Experienced JavaFX devs welcomed to suggest solutions to this
        private static OktaAwsCliEnvironment ENVIRONMENT;

        // The value of samlResponse is only valid if USER_AUTH_COMPLETE has counted down to zero
        /*private static readonly AtomicReference<String> samlResponse = new AtomicReference<>();

        public static String Login(OktaAwsCliEnvironment environment)
        {
            ENVIRONMENT = environment;

            launch();

            USER_AUTH_COMPLETE.await();
            return samlResponse.get();
        }

        public void start(final Stage stage)
        {
            stage.setWidth(802);
            stage.setHeight(650);
            Scene scene = new Scene(new Group());


            final WebView browser = new WebView();
            final WebEngine webEngine = browser.getEngine();

            ScrollPane scrollPane = new ScrollPane();
            scrollPane.setContent(browser);

            URI uri = URI.create(ENVIRONMENT.oktaAwsAppUrl);
            Map<String, List<String>> headers = new LinkedHashMap<>();
            java.net.CookieHandler.getDefault().put(uri, headers);

            webEngine.getLoadWorker().stateProperty()
                        .addListener((ov, oldState, newState)-> {
                if (newState == State.SUCCEEDED)
                {
                    checkForAwsSamlSignon(stage, webEngine, uri, headers);
                    stage.setTitle(webEngine.getLocation());
                }
            });
            webEngine.load(uri.toASCIIString());

            scene.setRoot(scrollPane);

            stage.setScene(scene);
            stage.show();
        }

        private void checkForAwsSamlSignon(Stage stage, WebEngine webEngine, URI uri, Map<String, List<String>> headers)
        {
            String samlResponseForAws = getSamlResponseForAws(webEngine.getDocument());
            if (samlResponseForAws != null)
            {
                finishAuthentication(stage, uri, headers, samlResponseForAws);
            }
        }

        private String getSamlResponseForAws(Document document)
        {
            Node awsStsSamlForm = getAwsStsSamlForm(document);
            if (awsStsSamlForm == null) return null;
            return getSamlResponseFromForm(awsStsSamlForm);
        }

        private Node getAwsStsSamlForm(Document document)
        {
            NodeList formNodes = document.getElementsByTagName("form");
            for (Node form : new NodeListIterable(formNodes))
            {
                NamedNodeMap formAttributes = form.getAttributes();
                if (formAttributes == null) continue;
                Node formActionAttribute = formAttributes.getNamedItem("action");
                if (formActionAttribute == null) continue;
                String formAction = formActionAttribute.getTextContent();
                if ("https://signin.aws.amazon.com/saml".equals(formAction))
                {
                    return form;
                }
            }
            return null;
        }

        private String getSamlResponseFromForm(@Nonnull Node awsStsSamlForm)
        {
            Node samlResponseInput = getSamlResponseInput(awsStsSamlForm);
            if (samlResponseInput == null)
                throw new IllegalStateException("Request to AWS STS SAML endpoint missing SAMLResponse");
            NamedNodeMap attributes = samlResponseInput.getAttributes();
            Node value = attributes.getNamedItem("value");
            return value.getTextContent();
        }

        private Node getSamlResponseInput(@Nonnull Node parent)
        {
            for (Node child : new NodeListIterable(parent.getChildNodes()))
            {
                if (isSamlResponseInput(child))
                {
                    return child;
                }
                else
                {
                    Node samlResponseInput = getSamlResponseInput(child);
                    if (samlResponseInput != null) return samlResponseInput;
                }
            }
            return null;
        }

        private boolean isSamlResponseInput(@Nonnull Node child)
        {
            boolean isInput = "input".equals(child.getLocalName());
            if (!isInput) return false;
            NamedNodeMap attributes = child.getAttributes();
            if (attributes == null) return false;
            Node nameAttribute = attributes.getNamedItem("name");
            if (nameAttribute == null) return false;
            String name = nameAttribute.getTextContent();
            return "SAMLResponse".equals(name);
        }

        private void finishAuthentication(Stage stage, URI uri, Map<String, List<String>> headers, String samlResponseForAws)
        {
            samlResponse.set(samlResponseForAws);
            try
            {
                CookieStore cookieStore = extractCookies(uri, headers);
                CookieHelper.storeCookies(cookieStore);
            }
            catch (IOException e)
            {
                throw new RuntimeException(e);
            }
            finally
            {
                stage.close();
                USER_AUTH_COMPLETE.countDown();
            }
        }

        private CookieStore extractCookies(URI uri, Map<String, List<String>> headers)
        {
            List<String> cookieHeaders = CookieHandler.getDefault().get(uri, headers).get("Cookie");
            return CookieHelper.parseCookies(cookieHeaders);
        }*/
    }
}
