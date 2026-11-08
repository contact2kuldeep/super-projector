import type { ProjectionResponse } from '../api/projectionClient'

// Create formatters once and reuse them. 'en-AU' gives Australian formatting,
// e.g. 1932437.34 -> "$1,932,437.34".
const currency = new Intl.NumberFormat('en-AU', { style: 'currency', currency: 'AUD' })
const percent = new Intl.NumberFormat('en-AU', { maximumFractionDigits: 2 })

type Props = {
  result: ProjectionResponse
}

/** Shows a successful projection. */
export function ProjectionResult({ result }: Props) {
  const { assumptions } = result

  return (
    <article className="card result" aria-labelledby="result-heading">
      <h2 id="result-heading">Projected balance at {result.retirementAge}</h2>
      <p className="balance">{currency.format(result.projectedBalance)}</p>

      {/* A description list suits label/value pairs. */}
      <dl className="stats">
        <div>
          <dt>Years to retirement</dt>
          <dd>{result.yearsToRetirement}</dd>
        </div>
        <div>
          <dt>Total contributions (after tax)</dt>
          <dd>{currency.format(result.totalContributions)}</dd>
        </div>
        <div>
          <dt>Investment earnings</dt>
          <dd>{currency.format(result.totalInvestmentEarnings)}</dd>
        </div>
      </dl>

      <h3>Assumptions</h3>
      <ul className="assumptions">
        <li>Net investment return: {percent.format(assumptions.netReturnPercent)}% a year</li>
        <li>Wage growth: {percent.format(assumptions.wageGrowthPercent)}% a year</li>
        <li>Contributions tax: {percent.format(assumptions.contributionsTaxPercent)}%</li>
        <li>Retirement age: {assumptions.retirementAge}</li>
      </ul>

      <p className="disclaimer">{result.disclaimer}</p>
    </article>
  )
}
