# Copilot Instructions

## Shared Instructions

Shared Copilot instructions, skills and prompts are maintained centrally in the public
[account-level `.github` repository](https://github.com/f2calv/.github). They are deliberately not
copied here.

If those shared files are unavailable, stop rather than guessing the conventions.

## Repository Role

- Keep this repository transport-neutral: it owns voice processing, not messaging or agent behavior.
- Keep packaging limited to the library and its `CasCap.Api.Voice.Testing` companion of test fakes.
- Never commit audio recordings, transcripts, credentials, private endpoints or identifying media metadata.
- Keep live integration tests credential-optional and tag them `Category=LiveProvider` to exclude
  them from the default credential-free CI run. Offline configuration integration tests can run in CI.
