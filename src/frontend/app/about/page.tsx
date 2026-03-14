import type { Metadata } from "next";

export const metadata: Metadata = {
  title: "Um — Ez.Reasons",
  description: "Um Ez.Reasons verkefnið — nafnlaus uppörvandi bréf til Íslendinga.",
};

export default function AboutPage() {
  return (
    <div className="mx-auto max-w-2xl px-4 py-10 sm:px-6 sm:py-16">
      <h1 className="mb-6 text-3xl font-bold tracking-tight text-foreground sm:text-4xl">
        Um Ez.Reasons
      </h1>

      <div className="space-y-4 leading-relaxed text-foreground/85">
        <p>
          Ez.Reasons er vettvangur þar sem fólk getur deilt nafnlausum
          uppörvandi bréfum. Hugmyndin er einföld: stundum þarf maður bara að
          lesa eitthvað fallegt sem einhver annar hefur skrifað.
        </p>

        <p>
          Á Íslandi erum við oft dugleg að halda okkur fyrir sjálf en stundum
          getur lítið bréf frá ókunnugum breytt deginum. Hvort sem þú átt
          erfiðan dag eða vilt bara dreifa jákvæðni, þá er þetta staðurinn
          fyrir þig.
        </p>

        <h2 className="pt-4 text-xl font-semibold text-foreground">
          Hvernig virkar þetta?
        </h2>

        <ul className="list-inside list-disc space-y-2 text-foreground/80">
          <li>
            <strong>Lestu bréf</strong> — Smelltu á &ldquo;Næsta bréf&rdquo;
            til að fá handahófskennt uppörvandi bréf.
          </li>
          <li>
            <strong>Skrifaðu bréf</strong> — Deildu eigin uppörvunarorðum. Öll
            bréf eru nafnlaus og fara í gegnum yfirferð áður en þau birtast.
          </li>
          <li>
            <strong>Gefðu endurgjöf</strong> — Segðu okkur hvort bréfið hafi
            gert þér gott, svo við getum bætt upplifunina.
          </li>
        </ul>

        <h2 className="pt-4 text-xl font-semibold text-foreground">
          Reglur
        </h2>

        <p>
          Öll bréf eru yfirfarin af ritstjórum áður en þau birtast. Bréf sem
          innihalda hatursáróður, auglýsingar eða óviðeigandi efni verða hafnað.
          Markmiðið er að skapa öruggan og jákvæðan vettvang fyrir alla.
        </p>

        <div className="mt-8 rounded-xl border border-accent-light bg-accent-light/30 p-6 text-center">
          <p className="text-lg font-medium text-foreground">
            &ldquo;Eitt fallegt orð getur breytt einhvers degi.&rdquo;
          </p>
        </div>
      </div>
    </div>
  );
}
