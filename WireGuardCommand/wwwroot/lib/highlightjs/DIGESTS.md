## Subresource Integrity

If you are loading Highlight.js via CDN you may wish to use [Subresource Integrity](https://developer.mozilla.org/en-US/docs/Web/Security/Subresource_Integrity) to guarantee that you are using a legimitate build of the library.

To do this you simply need to add the `integrity` attribute for each JavaScript file you download via CDN. These digests are used by the browser to confirm the files downloaded have not been modified.

```html
<script
  src="//cdnjs.cloudflare.com/ajax/libs/highlight.js/11.10.0/highlight.min.js"
  integrity="sha384-pGqTJHE/m20W4oDrfxTVzOutpMhjK3uP/0lReY0Jq/KInpuJSXUnk4WAYbciCLqT"></script>
<!-- including any other grammars you might need to load -->
<script
  src="//cdnjs.cloudflare.com/ajax/libs/highlight.js/11.10.0/languages/go.min.js"
  integrity="sha384-Mtb4EH3R9NMDME1sPQALOYR8KGqwrXAtmc6XGxDd0XaXB23irPKsuET0JjZt5utI"></script>
```

The full list of digests for every file can be found below.

### Digests

```
sha384-0StLGSBIhoerTxrjwG/Lx1LYO/qmSp2TzCqgzCmnBDrVmkkeaFW9vHKuLHK5Ue7H /es/languages/ini.js
sha384-Qk1583V3PAnmXJ00e8ufkLJOuIZIrqrg5sTGoghEOwikzdWrdpiJv8lQqrURXjBG /es/languages/ini.min.js
sha384-l2Aa/1StxIePW3t8ALFDwO/VZShzdfn5Y+0qIFkvO4WXem4DA5+6fgKQW+w/xKEk /languages/ini.js
sha384-0/1VV9gfjl+ZuUf+R7fvp6dQlJ5JVh+WzEqjzOwd+PCh8fa104Vm13MBaJjTz+cG /languages/ini.min.js
sha384-4J+xU9gwIGgbjxs71wnX57uHA9wtOrOJl5Qmq/reY42neO7/2G31oYEH7C0+bl5v /highlight.js
sha384-dpz+wEMErm3sIp8sn1sT4K0QGAwf2VDB2xQWp2weNz0UL9LV5/j5ttygMFpSbRRs /highlight.min.js
```

